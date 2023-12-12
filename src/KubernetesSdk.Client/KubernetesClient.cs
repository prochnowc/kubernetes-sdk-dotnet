// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Client.Diagnostics;
using Kubernetes.Client.Http;
using Kubernetes.Client.Operations;
using Kubernetes.Serialization;
using Polly.Retry;

namespace Kubernetes.Client;

/// <summary>
/// Represents the Kubernetes API client.
/// </summary>
public class KubernetesClient : IDisposable
{
    private readonly KubernetesClientOptions _options;
    private readonly ConcurrentDictionary<Type, KubernetesClientOperations> _operations = new ();
#pragma warning disable IDISP008
    private readonly HttpClient _httpClient;
#pragma warning restore IDISP008
    private readonly bool _disposeHttpClient;
    private readonly IKubernetesSerializerFactory _serializerFactory;
    private readonly KubernetesClientMetrics _metrics;

    /// <summary>
    /// Gets the <see cref="KubernetesClientOptions"/>.
    /// </summary>
    public KubernetesClientOptions Options => _options;

    /// <summary>
    /// Gets the <see cref="IKubernetesSerializerFactory"/>.
    /// </summary>
    public IKubernetesSerializerFactory SerializerFactory => _serializerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesClient"/> class using the
    /// <see cref="DefaultOptionsProvider"/> and <see cref="KubernetesSerializerFactory.Instance"/>.
    /// </summary>
    public KubernetesClient()
        : this(new DefaultOptionsProvider().CreateOptions(), KubernetesSerializerFactory.Instance)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesClient"/> class using the specified
    /// <see cref="KubernetesClientOptions"/> and <see cref="KubernetesSerializerFactory"/>.
    /// </summary>
    /// <param name="options">The <see cref="KubernetesClientOptions"/>.</param>
    /// <param name="serializerFactory">The <see cref="IKubernetesSerializerFactory"/>.</param>
    public KubernetesClient(KubernetesClientOptions options, IKubernetesSerializerFactory serializerFactory)
        : this(options, serializerFactory, KubernetesHttpClientFactory.CreateHttpClient(options), true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesClient"/> class using the specified
    /// HTTP client factory,  <see cref="KubernetesClientOptions"/> and <see cref="KubernetesSerializerFactory"/>.
    /// </summary>
    /// <param name="options">The <see cref="KubernetesClientOptions"/>.</param>
    /// <param name="serializerFactory">The <see cref="IKubernetesSerializerFactory"/>.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
    /// <param name="disposeHttpClient">Whether to dispose the <see cref="HttpClient"/>.</param>
    public KubernetesClient(
        KubernetesClientOptions options,
        IKubernetesSerializerFactory serializerFactory,
        HttpClient httpClient,
        bool disposeHttpClient)
    {
        Ensure.Arg.NotNull(options);
        Ensure.Arg.NotNull(serializerFactory);
        Ensure.Arg.NotNull(httpClient);

        options = options.Seal();

        _options = options;
        _serializerFactory = serializerFactory;
        _httpClient = httpClient;
        _disposeHttpClient = disposeHttpClient;
        _metrics = new KubernetesClientMetrics(_httpClient);
    }

    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument", Justification = "Passed by caller.")]
    private Activity? StartActivity(
        KubernetesRequest request,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        return KubernetesClientDefaults.ActivitySource.StartActivity(
            this,
            $"K8s {request.Action ?? request.Method.ToString()}",
            ActivityKind.Client,
            () =>
            {
                TagList tags = request.GetRequestTags();
                Uri? peer = _httpClient.BaseAddress;
                tags.Add(OtelTags.NetPeerName, peer?.Host);
                tags.Add(OtelTags.NetPeerPort, peer?.IsDefaultPort == true ? null : peer?.Port);
                return tags;
            },
            memberName,
            filePath,
            lineNumber);
    }

    /// <summary>
    /// Sends a request to the Kubernetes API server.
    /// </summary>
    /// <param name="request">The <see cref="KubernetesRequest"/> to send.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="KubernetesResponse"/>.</returns>
    /// <exception cref="KubernetesRequestException">Server returned an error or there was an error processing the response.</exception>
    /// <exception cref="TaskCanceledException">Request timed out or was canceled.</exception>
    public virtual async Task<KubernetesResponse> SendAsync(KubernetesRequest request, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(request);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        TimeSpan timeout = request.Timeout ?? _options.HttpClientTimeout;
        cts.CancelAfter(timeout);
        cancellationToken = cts.Token;

        using Activity? activity = StartActivity(request);

        KubernetesResponse? response = null;
        try
        {
            AsyncRetryPolicy<HttpResponseMessage> policy =
                Options.HttpClientRetryPolicy ?? KubernetesClientDefaults.HttpClientRetryPolicy;

            using HttpRequestMessage httpRequest = request.CreateHttpRequest(_options, _serializerFactory);

            [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "Diposed after SendRequest returns.")]
            async Task<HttpResponseMessage> SendRequest(CancellationToken ct)
            {
                return await _httpClient.SendAsync(
                    httpRequest,
                    HttpCompletionOption.ResponseHeadersRead,
                    ct);
            }

            using (KubernetesClientMetrics.TrackedRequest trackedRequest = _metrics.TrackRequest(request))
            {
                HttpResponseMessage httpResponse = await policy.ExecuteAsync(SendRequest, cancellationToken)
                                                               .ConfigureAwait(false);

                response = new KubernetesResponse(request, httpResponse, _serializerFactory);
                trackedRequest.Complete(response);
            }

            await response.EnsureSuccessStatusCodeAsync(cancellationToken)
                          .ConfigureAwait(false);

            activity?.SetStatus(ActivityStatusCode.Ok);
            return response;
        }
        catch (Exception error)
        {
            activity?.SetStatus(ActivityStatusCode.Error, error.Message);
            response?.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Connects to a WebSocket endpoint of the Kubernetes API server.
    /// </summary>
    /// <param name="uri">The URI to connect to.</param>
    /// <param name="protocol">The sub-protocol to use.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="KubernetesWebSocket"/>.</returns>
    public async Task<KubernetesWebSocket> ConnectAsync(
        Uri uri,
        string? protocol,
        CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(uri);

        var webSocket = new KubernetesWebSocket(_httpClient, protocol, _options);
        await webSocket.ConnectAsync(uri, cancellationToken)
                       .ConfigureAwait(false);

        return webSocket;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal T GetOperations<T>(Func<KubernetesClient, T> factory)
        where T : KubernetesClientOperations
    {
        Ensure.Arg.NotNull(factory);

        Type type = typeof(T);

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        return (T)_operations.GetOrAdd(type, static (_, arg) => arg.factory(arg.client), (client: this, factory));
#else
        return (T)_operations.GetOrAdd(type, _ => factory(this));
#endif
    }

    /// <summary>
    /// Invoked when the object is disposed.
    /// </summary>
    /// <param name="disposing">Whether to dispose unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_disposeHttpClient)
            {
                _httpClient.Dispose();
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}