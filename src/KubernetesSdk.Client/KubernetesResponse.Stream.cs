// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Client;

/// <content>
/// Contains stream related methods for the <see cref="KubernetesResponse"/> class.
/// </content>
public partial class KubernetesResponse
{
    /// <summary>
    /// Wrapper for the content stream to correctly dispose the HttpResponseMessage.
    /// </summary>
    internal sealed class WrappedStream : Stream
    {
        private readonly HttpResponseMessage _httpResponse;
        private readonly Stream _stream;

        private WrappedStream(HttpResponseMessage httpResponse, Stream stream)
        {
            _httpResponse = httpResponse;
            _stream = stream;
        }

        public static async Task<WrappedStream> CreateAsync(HttpResponseMessage httpResponse, CancellationToken cancellationToken)
        {
            Stream stream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken)
                                              .ConfigureAwait(false);

            return new WrappedStream(httpResponse, stream);
        }

        public override IAsyncResult BeginRead(
            byte[] buffer,
            int offset,
            int count,
            AsyncCallback? callback,
            object? state)
        {
            return _stream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(
            byte[] buffer,
            int offset,
            int count,
            AsyncCallback? callback,
            object? state)
        {
            return _stream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            _stream.Close();
        }

#if NETSTANDARD2_1_OR_GREATER
        public override void CopyTo(Stream destination, int bufferSize)
        {
            _stream.CopyTo(destination, bufferSize);
        }
#endif

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return _stream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        [SuppressMessage(
            "IDisposableAnalyzers.Correctness",
            "IDISP007:Don\'t dispose injected",
            Justification = "Ownership is transferred from KubernetesResponse.")]
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _stream.Dispose();
                _httpResponse.Dispose();
            }
        }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        [SuppressMessage(
            "IDisposableAnalyzers.Correctness",
            "IDISP007:Don\'t dispose injected",
            Justification = "Ownership is transferred from KubernetesResponse.")]
        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            await _stream.DisposeAsync();
            _httpResponse.Dispose();
        }
#endif

        public override int EndRead(IAsyncResult asyncResult)
        {
            return _stream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            _stream.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _stream.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

#if NETSTANDARD2_1_OR_GREATER
        public override int Read(Span<byte> buffer)
        {
            return _stream.Read(buffer);
        }
#endif

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

#if NETSTANDARD2_1_OR_GREATER
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return _stream.ReadAsync(buffer, cancellationToken);
        }
#endif

        public override int ReadByte()
        {
            return _stream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

#if NETSTANDARD2_1_OR_GREATER
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            _stream.Write(buffer);
        }
#endif

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _stream.WriteAsync(buffer, offset, count, cancellationToken);
        }

#if NETSTANDARD2_1_OR_GREATER
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return _stream.WriteAsync(buffer, cancellationToken);
        }
#endif

        public override void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanTimeout => _stream.CanTimeout;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length => _stream.Length;

        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public override int ReadTimeout
        {
            get => _stream.ReadTimeout;
            set => _stream.ReadTimeout = value;
        }

        public override int WriteTimeout
        {
            get => _stream.WriteTimeout;
            set => _stream.WriteTimeout = value;
        }
    }
}
