using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Kubernetes.Serialization;

namespace Kubernetes.Client;

public sealed class KubernetesRequest
{
    public HttpMethod Method { get; }

    public Uri Uri { get; }

    public object? Content { get; set; }

    public KubernetesRequest(HttpMethod method, IReadOnlyCollection<string> consumes, IReadOnlyCollection<string> produces, Uri uri)
    {
        Method = method;
        Uri = uri;
    }

    internal HttpRequestMessage CreateHttpRequest(
        KubernetesClientOptions options,
        IKubernetesSerializerFactory serializerFactory)
    {
        HttpContent? content = null;
        if (Content != null)
        {
            string contentType = "application/json";
            IKubernetesSerializer serializer = serializerFactory.CreateSerializer(contentType);
            content = new StringContent(serializer.Serialize(Content));
        }

        var request = new HttpRequestMessage(Method, Uri)
        {
            Content = content,
        };

        // TODO: below code can probably go to KubernetesHttpClientFactory
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        request.Version = options.DisableHttp2
            ? HttpVersion.Version11
            : HttpVersion.Version20;
#endif

        if (!string.IsNullOrWhiteSpace(options.UserAgent))
        {
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue(options.UserAgent));
        }

        if (!string.IsNullOrWhiteSpace(options.TlsServerName))
        {
            request.Headers.Host = options.TlsServerName;
        }

        return request;
    }
}