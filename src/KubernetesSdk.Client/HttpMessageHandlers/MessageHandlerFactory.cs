using System;
using System.Net.Http;

namespace Kubernetes.Client.HttpMessageHandlers;

public static partial class MessageHandlerFactory
{
    public static DelegatingHandler CreateAuthenticationMessageHandler(KubernetesClientOptions options)
    {
        if (options.TokenProvider != null)
            return new TokenAuthenticationHandler(options.TokenProvider);

        if (!string.IsNullOrWhiteSpace(options.AccessToken))
            return new TokenAuthenticationHandler(options.AccessToken!);

        if (!string.IsNullOrWhiteSpace(options.Username))
            return new BasicAuthenticationHandler(options.Username!, options.Password ?? string.Empty);

        // certificate authentication uses no authentication header so we dont need a handler
        return new NoOpMessageHandler();
    }

    private sealed class HttpMessageHandlerWrapper : DelegatingHandler
    {
        private readonly Action? _disposeCallback;

        public HttpMessageHandlerWrapper(HttpMessageHandler handler, Action? disposeCallback)
            : base(handler)
        {
            _disposeCallback = disposeCallback;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _disposeCallback?.Invoke();
            }
        }
    }

    private sealed class NoOpMessageHandler : DelegatingHandler
    {
    }
}