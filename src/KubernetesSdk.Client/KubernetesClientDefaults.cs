// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

namespace Kubernetes.Client;

internal static class KubernetesClientDefaults
{
    private static readonly Lazy<ProductInfoHeaderValue> LazyUserAgent = new (
        () =>
        {
            var versionAttribute =
                typeof(KubernetesClientDefaults).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            string? version = versionAttribute?.InformationalVersion;
            return new ProductInfoHeaderValue("Kubernetes_NET_SDK", version);
        });

    public static ProductInfoHeaderValue UserAgent => LazyUserAgent.Value;

    public static AsyncRetryPolicy<HttpResponseMessage> HttpClientRetryPolicy { get; } =
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))
                                             + TimeSpan.FromMilliseconds(ConcurrentRandom.Next(0, 1000)));
}