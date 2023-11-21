// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Kubernetes.Client.KubeConfig;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kubernetes.Client.Extensions.DependencyInjection;

/// <summary>
/// Provides the builder for configuring <see cref="KubernetesClient"/> instances.
/// </summary>
public sealed class KubernetesClientBuilder
{
    /// <summary>
    /// Gets the <see cref="IServiceCollection"/>.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Gets the name of the client.
    /// </summary>
    public string Name { get; }

    internal KubernetesClientBuilder(IServiceCollection services, string name)
    {
        Services = services;
        Name = name;
    }

    /// <summary>
    /// Configures the <see cref="IHttpClientBuilder"/>.
    /// </summary>
    /// <param name="configure">The delegate to configure the <see cref="IHttpClientBuilder"/>.</param>
    /// <returns>The <see cref="KubernetesClientBuilder"/>.</returns>
    public KubernetesClientBuilder ConfigureHttpClient(Action<IHttpClientBuilder> configure)
    {
        Ensure.Arg.NotNull(configure);
        configure(Services.AddHttpClient(KubernetesClientFactory.GetHttpClientName(Name)));
        return this;
    }

    /// <summary>
    /// Manually configures the <see cref="KubernetesClientOptions"/>.
    /// </summary>
    /// <remarks>
    /// Note that the default configuration will not be applied when using this method.
    /// </remarks>
    /// <param name="configure">The delegate to configure the <see cref="KubernetesClientOptions"/>.</param>
    /// <returns>The <see cref="KubernetesClientBuilder"/>.</returns>
    public KubernetesClientBuilder Configure(Action<KubernetesClientOptions> configure)
    {
        Services.Configure<KubernetesClientBuilderOptions>(o => o.UseDefaultConfig = false);

        Services.AddOptions<KubernetesClientOptions>(Name)
                .Configure(configure);

        return this;
    }

    /// <summary>
    /// Manually configures the <see cref="KubernetesClientOptions"/> using the specified
    /// <typeparamref name="TProvider"/>.
    /// </summary>
    /// <remarks>
    /// Note that the default configuration will not be applied when using this method.
    /// </remarks>
    /// <param name="configure">The delegate used to configure the <see cref="IKubernetesClientOptionsProvider"/>.</param>
    /// <typeparam name="TProvider">The type of the <see cref="IKubernetesClientOptionsProvider"/> to use.</typeparam>
    /// <returns>The <see cref="KubernetesClientBuilder"/>.</returns>
    public KubernetesClientBuilder ConfigureFrom<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TProvider>(
        Action<TProvider>? configure = null)
        where TProvider : class, IKubernetesClientOptionsProvider
    {
        Services.Configure<KubernetesClientBuilderOptions>(o => o.UseDefaultConfig = false);

        Services.TryAddTransient<TProvider>();

        Services.AddOptions<KubernetesClientOptions>(Name)
                .Configure<TProvider>(
                    (o, provider) =>
                    {
                        configure?.Invoke(provider);
                        provider.BindOptions(o);
                    });

        return this;
    }

    /// <summary>
    /// Configures the <see cref="KubernetesClientOptions"/> using the <see cref="DefaultOptionsProvider"/>.
    /// </summary>
    /// <param name="configure">The delegate to configure the <see cref="DefaultOptionsProvider"/>.</param>
    /// <returns>The <see cref="KubernetesClientBuilder"/>.</returns>
    public KubernetesClientBuilder ConfigureFromDefault(Action<DefaultOptionsProvider>? configure = null)
    {
        return ConfigureFrom(configure);
    }

    /// <summary>
    /// Configures the <see cref="KubernetesClientOptions"/> using the <see cref="KubeConfigOptionsProvider"/>.
    /// </summary>
    /// <param name="configure">The delegate to configure the <see cref="KubeConfigOptionsProvider"/>.</param>
    /// <returns>The <see cref="KubernetesClientBuilder"/>.</returns>
    public KubernetesClientBuilder ConfigureFromKubeConfig(Action<KubeConfigOptionsProvider>? configure = null)
    {
        return ConfigureFrom(configure);
    }

    /// <summary>
    /// Configures the <see cref="KubernetesClientOptions"/> using the <see cref="KubeConfigOptionsProvider"/>.
    /// </summary>
    /// <param name="configPath">The path to the kubeconfig file.</param>
    /// <param name="context">The name of the cluster context to use.</param>
    /// <returns>The <see cref="KubernetesClientBuilder"/>.</returns>
    public KubernetesClientBuilder ConfigureFromKubeConfig(string? configPath, string? context)
    {
        return ConfigureFromKubeConfig(
            p =>
            {
                p.ConfigPath = configPath;
                p.Context = context;
            });
    }

    /// <summary>
    /// Configures the <see cref="KubernetesClientOptions"/> using the <see cref="InClusterOptionsProvider"/>.
    /// </summary>
    /// <returns>The <see cref="KubernetesClientBuilder"/>.</returns>
    public KubernetesClientBuilder ConfigureFromCluster()
    {
        return ConfigureFrom<InClusterOptionsProvider>();
    }
}