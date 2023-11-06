// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Client.Operations;
using Kubernetes.Serialization;

namespace Kubernetes.Client;

/// <summary>
/// Represents the Kubernetes API client.
/// </summary>
public class KubernetesClient
{
    private readonly KubernetesClientOptions _options;
    private readonly ConcurrentDictionary<Type, KubernetesClientOperations> _operations = new ();
    private readonly HttpClient _httpClient;
    private readonly IKubernetesSerializerFactory _serializerFactory;

    /// <summary>
    /// Gets the <see cref="KubernetesClientOptions"/>.
    /// </summary>
    public KubernetesClientOptions Options => _options;

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
        : this(options, serializerFactory, KubernetesHttpClientFactory.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesClient"/> class using the specified
    /// HTTP client factory,  <see cref="KubernetesClientOptions"/> and <see cref="KubernetesSerializerFactory"/>.
    /// </summary>
    /// <param name="options">The <see cref="KubernetesClientOptions"/>.</param>
    /// <param name="serializerFactory">The <see cref="IKubernetesSerializerFactory"/>.</param>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    public KubernetesClient(
        KubernetesClientOptions options,
        IKubernetesSerializerFactory serializerFactory,
        Func<KubernetesClientOptions, HttpClient> httpClientFactory)
    {
        Ensure.Arg.NotNull(options);
        Ensure.Arg.NotNull(serializerFactory);
        Ensure.Arg.NotNull(httpClientFactory);

        _options = options;
        _serializerFactory = serializerFactory;
        _httpClient = httpClientFactory(options);
    }

    /// <summary>
    /// Sends a request to the Kubernetes API server.
    /// </summary>
    /// <param name="request">The <see cref="KubernetesRequest"/> to send.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="KubernetesResponse"/>.</returns>
    /// <exception cref="KubernetesRequestException">Server returned an error or the was an error processing the response.</exception>
    /// <exception cref="TaskCanceledException">Request timed out or was canceled.</exception>
    public virtual async Task<KubernetesResponse> SendAsync(KubernetesRequest request, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(request);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        TimeSpan timeout = request.Timeout ?? _options.HttpClientTimeout;
        cts.CancelAfter(timeout);
        cancellationToken = cts.Token;

        using HttpRequestMessage httpRequest = request.CreateHttpRequest(_options, _serializerFactory);

        KubernetesResponse? response = null;
        try
        {
            HttpResponseMessage httpResponse =
                await _httpClient.SendAsync(
                                     httpRequest,
                                     HttpCompletionOption.ResponseHeadersRead,
                                     cancellationToken)
                                 .ConfigureAwait(false);

            response = new KubernetesResponse(httpResponse, _serializerFactory);
            await response.EnsureSuccessStatusCodeAsync(cancellationToken)
                          .ConfigureAwait(false);

            return response;
        }
        catch
        {
            response?.Dispose();
            throw;
        }
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
}