// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;

namespace Kubernetes.Client.Http;

/// <summary>
/// Provides a factory for creating <see cref="HttpClient"/>s used by the <see cref="KubernetesClient"/>.
/// </summary>
public static partial class KubernetesHttpClientFactory
{
    internal static DelegatingHandler CreateAuthenticationMessageHandler(KubernetesClientOptions options)
    {
        DelegatingHandler? result;
        if (options.TokenProvider != null)
        {
            result = new TokenAuthenticationHandler(options.TokenProvider);
        }
        else if (!string.IsNullOrWhiteSpace(options.AccessToken))
        {
            result = new TokenAuthenticationHandler(options.AccessToken);
        }
        else if (!string.IsNullOrWhiteSpace(options.Username))
        {
            result = new BasicAuthenticationHandler(options.Username, options.Password ?? string.Empty);
        }
        else
        {
            result = new NoOpMessageHandler();
        }

        return result;
    }

    /// <summary>
    /// Creates additional message handlers used by the <see cref="HttpClient"/> of a <see cref="KubernetesClient"/>.
    /// </summary>
    /// <param name="options">The <see cref="KubernetesClientOptions"/>.</param>
    /// <returns>The chain of <see cref="DelegatingHandler"/>'s.</returns>
    [SuppressMessage(
        "IDisposableAnalyzers.Correctness",
        "IDISP003:Dispose previous before re-assigning",
        Justification = "InnerHandler is disposed by the DelegatingHandler.")]
    public static DelegatingHandler CreateMessageHandlers(KubernetesClientOptions options)
    {
        Ensure.Arg.NotNull(options);

        DelegatingHandler? result = null;
        DelegatingHandler? previous = null;
        foreach (Func<KubernetesClientOptions, DelegatingHandler> handlerFactory in options.HttpMessageHandlers)
        {
            DelegatingHandler handler = handlerFactory(options);
            result ??= handler;
            if (previous != null)
            {
                handler.InnerHandler = previous;
            }

            previous = handler;
        }

        return result ?? new NoOpMessageHandler();
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

    /// <summary>
    /// Creates the <see cref="HttpClient"/> used by the <see cref="KubernetesClient"/>.
    /// </summary>
    /// <param name="options">The <see cref="KubernetesClientOptions"/>.</param>
    /// <returns>The <see cref="HttpClient"/>.</returns>
    [SuppressMessage(
        "IDisposableAnalyzers.Correctness",
        "IDISP003:Dispose previous before re-assigning",
        Justification = "handler is disposed by the DelegatingHandler chain.")]
    public static HttpClient CreateHttpClient(KubernetesClientOptions options)
    {
        Ensure.Arg.NotNull(options);

        HttpMessageHandler handler = CreatePrimaryMessageHandler(options);
        DelegatingHandler delegatingHandler = CreateMessageHandlers(options);
        delegatingHandler.InnerHandler = handler;
        var client = new HttpClient(delegatingHandler);
        ConfigureHttpClient(client, options);

        return client;
    }

    /// <summary>
    /// Configures the <see cref="HttpClient"/> used by the <see cref="KubernetesClient"/>.
    /// </summary>
    /// <param name="client">The <see cref="HttpClient"/> to configure.</param>
    /// <param name="options">The <see cref="KubernetesClientOptions"/>.</param>
    public static void ConfigureHttpClient(HttpClient client, KubernetesClientOptions options)
    {
        Ensure.Arg.NotNull(client);
        Ensure.Arg.NotNull(options);

        string host = !string.IsNullOrWhiteSpace(options.Host)
            ? options.Host
            : "https://localhost";

        client.BaseAddress = new Uri(
            host + (host.EndsWith("/")
                ? string.Empty
                : "/"));

        // Timeout is applied by KubernetesRequest to each individual request.
        client.Timeout = Timeout.InfiniteTimeSpan;
    }
}