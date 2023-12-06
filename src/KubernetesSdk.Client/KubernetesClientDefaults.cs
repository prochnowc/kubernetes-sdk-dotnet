// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

namespace Kubernetes.Client;

/// <summary>
/// Provides default values used by the Kubernetes client.
/// </summary>
public static class KubernetesClientDefaults
{
    /// <summary>
    /// The name used for diagnostics (tracing and metrics).
    /// </summary>
    public const string DiagnosticsName = "KubernetesSdk.Client";

    internal static string? Version { get; }
        = typeof(KubernetesClientDefaults)
          .Assembly
          .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
          .InformationalVersion;

    internal static readonly Meter Meter = new (DiagnosticsName, Version);

    internal static readonly ActivitySource ActivitySource = new (DiagnosticsName, Version);

    /// <summary>
    /// Gets the default user agent used by the Kubernetes client.
    /// </summary>
    public static ProductInfoHeaderValue UserAgent => new ("Kubernetes_NET_SDK", Version);

    /// <summary>
    /// Gets the default retry policy used by the Kubernetes client.
    /// </summary>
    public static AsyncRetryPolicy<HttpResponseMessage> HttpClientRetryPolicy { get; } =
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))
                                             + TimeSpan.FromMilliseconds(ConcurrentRandom.Next(0, 1000)));
}