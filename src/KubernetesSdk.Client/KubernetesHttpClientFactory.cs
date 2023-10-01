using System;
using System.Net.Http;
using System.Threading;
using Kubernetes.Client.HttpMessageHandlers;

namespace Kubernetes.Client;

internal static class KubernetesHttpClientFactory
{
    public static readonly Func<KubernetesClientOptions, HttpClient> Default = options =>
    {
        string host = !string.IsNullOrWhiteSpace(options.Host)
            ? options.Host!
            : "https://localhost";

        DelegatingHandler handler = MessageHandlerFactory.CreateAuthenticationMessageHandler(options);
        handler.InnerHandler = MessageHandlerFactory.CreatePrimaryHttpMessageHandler(options);

        return new HttpClient(handler)
        {
            BaseAddress = new Uri(host + (host.EndsWith("/") ? string.Empty : "/")),
            Timeout = Timeout.InfiniteTimeSpan,
        };
    };
}