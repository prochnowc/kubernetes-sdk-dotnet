// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Threading;
using Kubernetes.Client.HttpMessageHandlers;

namespace Kubernetes.Client;

/// <summary>
/// Provides a factory for creating <see cref="HttpClient"/>s used by the <see cref="KubernetesClient"/>.
/// </summary>
internal static class KubernetesHttpClientFactory
{
    public static readonly Func<KubernetesClientOptions, HttpClient> Default = options =>
    {
        string host = !string.IsNullOrWhiteSpace(options.Host)
            ? options.Host
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