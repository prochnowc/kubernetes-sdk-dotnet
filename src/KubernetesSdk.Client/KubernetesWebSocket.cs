// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipelines;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Client.Http;

namespace Kubernetes.Client;

/// <summary>
/// Provides a WebSocket to communicate with the Kubernetes API server.
/// </summary>
public sealed partial class KubernetesWebSocket : IDisposable, IAsyncDisposable
{
    private const int MaxFrameSize = 15 * 1024 * 1024; // 15MB

    private readonly HttpClient _httpClient;
    private readonly IClientWebSocket _webSocket;
    private readonly KubernetesClientOptions _options;
    private readonly Action? _disposeCallback;
    private readonly object _writeLock = new ();
    private readonly ConcurrentDictionary<int, WebSocketInputStream> _inputStreams = new ();
    private readonly ConcurrentDictionary<int, WebSocketOutputPipe> _outputPipes = new ();
    private readonly CancellationTokenSource _readerCancellationTokenSource = new ();
    private Task? _readerTask;

    private sealed class WebSocketInputStream : Stream
    {
        private readonly IClientWebSocket _webSocket;
        private readonly object _writeLock;
        private readonly int _channel;

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public WebSocketInputStream(IClientWebSocket webSocket, object writeLock, int channel)
        {
            _webSocket = webSocket;
            _writeLock = writeLock;
            _channel = channel;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Ensure.Arg.NotNull(buffer);

            ValueTask vt = WriteAsyncInternal(new ReadOnlyMemory<byte>(buffer, offset, count), CancellationToken.None);
            if (!vt.IsCompletedSuccessfully)
            {
                vt.AsTask()
                  .ConfigureAwait(false)
                  .GetAwaiter()
                  .GetResult();
            }
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            Ensure.Arg.NotNull(buffer);

            await WriteAsyncInternal(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken)
                .ConfigureAwait(false);
        }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            ValueTask vt = WriteAsyncInternal(buffer.ToArray(), CancellationToken.None);
            if (!vt.IsCompletedSuccessfully)
            {
                vt.AsTask()
                  .ConfigureAwait(false)
                  .GetAwaiter()
                  .GetResult();
            }
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return WriteAsyncInternal(buffer, cancellationToken);
        }
#endif

        [SuppressMessage(
            "Style",
            "VSTHRD200:Use \"Async\" suffix for async methods",
            Justification = "Just used internally")]
        private async ValueTask WriteAsyncInternal(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            byte[] writeBuffer = ArrayPool<byte>.Shared.Rent(Math.Min(buffer.Length, MaxFrameSize) + 1);

            try
            {
                writeBuffer[0] = (byte)_channel;
                for (int i = 0; i < buffer.Length; i += MaxFrameSize)
                {
                    int c = Math.Min(buffer.Length - i, MaxFrameSize);
                    buffer.Slice(i, c).CopyTo(writeBuffer.AsMemory(1));
                    var segment = new ArraySegment<byte>(writeBuffer, 0, c + 1);
                    Monitor.Enter(_writeLock);
                    try
                    {
                        await _webSocket.SendAsync(
                                            segment,
                                            WebSocketMessageType.Binary,
                                            i + MaxFrameSize < buffer.Length,
                                            cancellationToken)
                                        .ConfigureAwait(false);
                    }
                    finally
                    {
                        Monitor.Exit(_writeLock);
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(writeBuffer);
            }
        }
    }

    private sealed class WebSocketOutputPipe : IDisposable
    {
        private readonly Pipe _pipe;
        private readonly Stream _writeStream;
        private readonly Stream _readStream;
        private int _disposed;

        public Stream WriteStream => _writeStream;

        public Stream ReadStream => _readStream;

        public WebSocketOutputPipe()
        {
            _pipe = new Pipe(new PipeOptions(useSynchronizationContext: false));
            _readStream = _pipe.Reader.AsStream(true);
            _writeStream = _pipe.Writer.AsStream(true);
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;

            _pipe.Reader.Complete();
            _readStream.Dispose();
            _pipe.Writer.Complete();
            _writeStream.Dispose();
        }
    }

    internal static Func<IClientWebSocket> WebSocketFactory { get; } = () => new DefaultClientWebSocket();

    internal async Task ConnectAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(uri);

        Uri? baseUri = _httpClient.BaseAddress;

        if (!uri.IsAbsoluteUri)
        {
            if (baseUri == null)
                throw new ArgumentException("The URI must be absolute.", nameof(uri));

            uri = new Uri(baseUri, uri);
        }

        UriBuilder uriBuilder = new (uri)
        {
            Scheme = uri.Scheme == Uri.UriSchemeHttps
                ? "wss"
                : "ws",
        };

#if NET7_0_OR_GREATER
        await _webSocket.ConnectAsync(uriBuilder.Uri, _httpClient, cancellationToken)
                        .ConfigureAwait(false);
#else
        // we need to set the token on our own because the ClientWebSocket implementation
        // below .NET 7 does not use the HttpClient and it's HTTP message handlers.
        if (_options.TokenProvider != null)
        {
            string token = await _options.TokenProvider.GetTokenAsync(false, cancellationToken)
                                         .ConfigureAwait(false);

            var authenticationHeader = new AuthenticationHeaderValue(
                TokenAuthenticationHandler.AuthenticationScheme,
                token);

            _webSocket.Options.SetRequestHeader(HttpHeaderNames.Authorization, authenticationHeader.ToString());
        }

        await _webSocket.ConnectAsync(uriBuilder.Uri, null, cancellationToken)
                        .ConfigureAwait(false);
#endif

        CancellationToken ct = _readerCancellationTokenSource.Token;
        _readerTask = Task.Run(
            async () => await ReaderLoopAsync(ct)
                .ConfigureAwait(false));
    }

    private async Task ReaderLoopAsync(CancellationToken cancellationToken)
    {
        // Get a 64KB buffer
        byte[] buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);

        try
        {
            var segment = new ArraySegment<byte>(buffer);

            while (!cancellationToken.IsCancellationRequested && _webSocket.CloseStatus == null)
            {
                // We always get data in this format:
                // [stream index] (1 for stdout, 2 for stderr)
                // [payload]
                WebSocketReceiveResult result = await _webSocket.ReceiveAsync(segment, cancellationToken)
                                                                .ConfigureAwait(false);

                // Ignore empty messages
                if (result.Count < 2)
                {
                    continue;
                }

                int streamIndex = buffer[0];
                while (true)
                {
                    WebSocketOutputPipe stream = _outputPipes.GetOrAdd(streamIndex, _ => new WebSocketOutputPipe());
                    await stream.WriteStream.WriteAsync(buffer, 1, result.Count - 1, cancellationToken)
                                .ConfigureAwait(false);

                    if (result.EndOfMessage)
                    {
                        break;
                    }

                    result = await _webSocket.ReceiveAsync(segment, cancellationToken)
                                             .ConfigureAwait(false);
                }
            }
        }
        catch
        {
            // ignore
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);

            foreach (WebSocketOutputPipe b in _outputPipes.Values)
            {
                b.Dispose();
            }
        }
    }

    /// <summary>
    /// Aborts the connection and cancels any pending I/O operations.
    /// </summary>
    public void Abort()
    {
        _webSocket.Abort();
    }

    /// <summary>
    /// Gets the input stream for the specified channel.
    /// </summary>
    /// <remarks>
    /// <c>0</c> maps to stdin of the container.
    /// </remarks>
    /// <param name="channel">The channel number to get the stream for.</param>
    /// <returns>The <see cref="Stream"/>.</returns>
    public Stream GetInputStream(int channel)
    {
        if (channel < 0 || channel > byte.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(channel), channel, $"{nameof(channel)} must be >= 0");
        }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        return _inputStreams.GetOrAdd(
            channel,
            static (i, arg) => new WebSocketInputStream(arg.webSocket, arg.writeLock, i),
            (webSocket: _webSocket, writeLock: _writeLock));
#else
        return _inputStreams.GetOrAdd(channel, i => new WebSocketInputStream(_webSocket, _writeLock, i));
#endif
    }

    /// <summary>
    /// Gets the output stream for the specified channel.
    /// </summary>
    /// <remarks>
    /// <c>1</c> maps to stdout of the container.
    /// <c>2</c> maps to stderr of the container.
    /// </remarks>
    /// <param name="channel">The channel number to get the stream for.</param>
    /// <returns>The <see cref="Stream"/>.</returns>
    public Stream GetOutputStream(int channel)
    {
        if (channel < 0 || channel > byte.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(channel), channel, $"{nameof(channel)} must be >= 0");
        }

        WebSocketOutputPipe outputStream = _outputPipes.GetOrAdd(channel, static _ => new WebSocketOutputPipe());
        return outputStream.ReadStream;
    }

    /// <summary>
    /// Closes the connection.
    /// </summary>
    /// <param name="closeStatus">The close status.</param>
    /// <param name="statusDescription">The status description.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>The asynchronous task.</returns>
    public async Task CloseAsync(
        WebSocketCloseStatus closeStatus,
        string? statusDescription,
        CancellationToken cancellationToken = default)
    {
        await _webSocket.CloseAsync(closeStatus, statusDescription, cancellationToken)
                        .ConfigureAwait(false);
    }

    /// <inheritdoc />
    [SuppressMessage(
        "IDisposableAnalyzers.Correctness",
        "IDISP007:Don\'t dispose injected",
        Justification = "Ownership is transferred from the caller")]
    public void Dispose()
    {
        _readerCancellationTokenSource.Cancel();
        _readerCancellationTokenSource.Dispose();
        _readerTask?.Wait();
        _webSocket.Dispose();
        _disposeCallback?.Invoke();
    }

    /// <inheritdoc />
    [SuppressMessage(
        "IDisposableAnalyzers.Correctness",
        "IDISP007:Don\'t dispose injected",
        Justification = "Ownership is transferred from the caller")]
    [SuppressMessage(
        "Usage",
        "VSTHRD003:Avoid awaiting foreign Tasks",
        Justification = "Work is created in ConnectAsync()")]
    public async ValueTask DisposeAsync()
    {
        _readerCancellationTokenSource.Cancel();
        _readerCancellationTokenSource.Dispose();

        if (_readerTask != null)
        {
            await _readerTask.ConfigureAwait(false);
        }

        _webSocket.Dispose();
        _disposeCallback?.Invoke();
    }
}
