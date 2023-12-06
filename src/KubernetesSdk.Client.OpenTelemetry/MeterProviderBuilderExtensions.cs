// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using OpenTelemetry.Metrics;

namespace Kubernetes.Client.OpenTelemetry;

/// <summary>
/// Provides extension methods for <see cref="MeterProviderBuilder"/>.
/// </summary>
public static class MeterProviderBuilderExtensions
{
    /// <summary>
    /// Adds Kubernetes client metrics.
    /// </summary>
    /// <param name="builder">The <see cref="MeterProviderBuilder"/>.</param>
    /// <returns>The passed <see cref="MeterProviderBuilder"/>.</returns>
    public static MeterProviderBuilder AddKubernetesClientInstrumentation(this MeterProviderBuilder builder)
    {
        Ensure.Arg.NotNull(builder);
        return builder.AddMeter(KubernetesClientDefaults.DiagnosticsName);
    }
}