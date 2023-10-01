using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Client.Operations;
using Kubernetes.Serialization;

namespace Kubernetes.Client;

public class KubernetesClient
{
    private readonly KubernetesClientOptions _options;
    private readonly ConcurrentDictionary<Type, KubernetesClientOperations> _operations = new ();
    private readonly HttpClient _httpClient;
    private readonly IKubernetesSerializerFactory _serializerFactory;

    public KubernetesClient()
        : this(new DefaultOptionsProvider().CreateOptions(), KubernetesSerializerFactory.Instance)
    {
    }

    public KubernetesClient(KubernetesClientOptions options, IKubernetesSerializerFactory serializerFactory)
        : this(KubernetesHttpClientFactory.Default, options, serializerFactory)
    {
    }

    public KubernetesClient(
        Func<KubernetesClientOptions, HttpClient> httpClientFactory,
        KubernetesClientOptions options,
        IKubernetesSerializerFactory serializerFactory)
    {
        _options = options;
        _serializerFactory = serializerFactory;
        _httpClient = httpClientFactory(options);
    }

    public virtual async Task<KubernetesResponse> SendAsync(KubernetesRequest request, CancellationToken cancellationToken = default)
    {
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
    public T GetOperations<T>(Func<KubernetesClient, T> factory)
        where T : KubernetesClientOperations
    {
        Type type = typeof(T);

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        return (T)_operations.GetOrAdd(type, static (_, arg) => arg.factory(arg.client), (client: this, factory));
#else
        return (T)_operations.GetOrAdd(type, _ => factory(this));
#endif
    }
}