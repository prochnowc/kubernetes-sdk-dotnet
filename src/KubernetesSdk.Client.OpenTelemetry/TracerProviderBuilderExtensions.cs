// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using OpenTelemetry.Trace;

namespace Kubernetes.Client.OpenTelemetry;

/// <summary>
/// Provides extension methods for <see cref="TracerProviderBuilder"/>.
/// </summary>
public static class TracerProviderBuilderExtensions
{
    /// <summary>
    /// Adds Kubernetes client tracing.
    /// </summary>
    /// <param name="builder">The <see cref="TracerProviderBuilder"/>.</param>
    /// <returns>The passed <see cref="TracerProviderBuilder"/>.</returns>
    public static TracerProviderBuilder AddKubernetesClientInstrumentation(this TracerProviderBuilder builder)
    {
        Ensure.Arg.NotNull(builder);
        return builder.AddSource(KubernetesClientDefaults.DiagnosticsName);
    }
}