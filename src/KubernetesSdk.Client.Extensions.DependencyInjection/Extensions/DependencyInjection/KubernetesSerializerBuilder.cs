// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using Kubernetes.Serialization.Json;
using Kubernetes.Serialization.Yaml;
using Microsoft.Extensions.DependencyInjection;

namespace Kubernetes.Client.Extensions.DependencyInjection;

/// <summary>
/// Provides the builder for configuring Kubernetes serializers.
/// </summary>
public sealed class KubernetesSerializerBuilder
{
    /// <summary>
    /// Gets the <see cref="IServiceCollection"/>.
    /// </summary>
    public IServiceCollection Services { get; }

    internal KubernetesSerializerBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// Configures JSON serialization.
    /// </summary>
    /// <param name="configure">The delegate to configure the <see cref="KubernetesJsonOptions"/>.</param>
    /// <returns>The <see cref="KubernetesSerializerBuilder"/>.</returns>
    public KubernetesSerializerBuilder ConfigureJson(Action<KubernetesJsonOptions> configure)
    {
        Services.Configure(configure);
        return this;
    }

    /// <summary>
    /// Configures YAML serialization.
    /// </summary>
    /// <param name="configure">The delegate to configure the <see cref="KubernetesYamlOptions"/>.</param>
    /// <returns>The <see cref="KubernetesSerializerBuilder"/>.</returns>
    public KubernetesSerializerBuilder ConfigureYaml(Action<KubernetesYamlOptions> configure)
    {
        Services.Configure(configure);
        return this;
    }
}