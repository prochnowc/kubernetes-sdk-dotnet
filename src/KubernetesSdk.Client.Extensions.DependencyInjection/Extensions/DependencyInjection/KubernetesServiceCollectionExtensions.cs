// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Kubernetes.Client.Http;
using Kubernetes.Client.KubeConfig;
using Kubernetes.Serialization;
using Kubernetes.Serialization.Json;
using Kubernetes.Serialization.Yaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Kubernetes.Client.Extensions.DependencyInjection;

public static class KubernetesServiceCollectionExtensions
{
    private static T GetOptions<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(
        this IServiceProvider sp,
        string? name = null)
    {
        return sp.GetRequiredService<IOptionsMonitor<T>>()
                 .Get(name ?? Options.DefaultName);
    }

    public static KubernetesSerializerBuilder AddKubernetesSerializer(this IServiceCollection services)
    {
        Ensure.Arg.NotNull(services);

        services.TryAddEnumerable(
            new[]
            {
                ServiceDescriptor.Singleton<IKubernetesSerializerProvider, KubernetesJsonSerializerProvider>(
                    sp => new KubernetesJsonSerializerProvider(sp.GetOptions<KubernetesJsonOptions>())),
                ServiceDescriptor.Singleton<IKubernetesSerializerProvider, KubernetesYamlSerializerProvider>(
                    sp => new KubernetesYamlSerializerProvider(sp.GetOptions<KubernetesYamlOptions>())),
            });

        return new KubernetesSerializerBuilder(services);
    }

    private static void AddKubernetesClientCore(this IServiceCollection services)
    {
        AddKubernetesSerializer(services);

        services.TryAddEnumerable(
            new[]
            {
                ServiceDescriptor.Transient<IAuthProviderOptionsBinder, AzureAuthProviderOptionsBinder>(),
                ServiceDescriptor.Transient<IAuthProviderOptionsBinder, GcpAuthProviderOptionsBinder>(),
                ServiceDescriptor.Transient<IAuthProviderOptionsBinder, OidcAuthProviderOptionsBinder>(),
            });

        services.TryAddSingleton<IKubernetesSerializerFactory, KubernetesSerializerFactory>();
        services.TryAddSingleton<IKubernetesClientFactory, KubernetesClientFactory>();
    }

    public static KubernetesClientBuilder AddKubernetesClient(this IServiceCollection services, string name)
    {
        Ensure.Arg.NotNull(services);

        AddKubernetesClientCore(services);

        // Configure options using DefaultOptionsProvider only if no explicit configuration
        // is provided via KubernetesClientBuilder.Configure / KubernetesClientBuilder.ConfigureFrom
        services.AddOptions<KubernetesClientOptions>(name)
                .Configure<IServiceProvider>(
                    (o, sp) =>
                    {
                        var options = sp.GetOptions<KubernetesClientBuilderOptions>();
                        if (options.UseDefaultConfig)
                        {
                            var provider = ActivatorUtilities.CreateInstance<DefaultOptionsProvider>(sp);
                            provider.BindOptions(o);
                        }
                    });

        // Configure http client
        services.AddHttpClient(KubernetesClientFactory.GetHttpClientName(name))
                .ConfigurePrimaryHttpMessageHandler(
                    sp =>
                    {
                        var options = sp.GetOptions<KubernetesClientOptions>(name);
                        return KubernetesHttpClientFactory.CreatePrimaryMessageHandler(options);
                    })
                .AddHttpMessageHandler(
                    sp =>
                    {
                        var options = sp.GetOptions<KubernetesClientOptions>(name);
                        return KubernetesHttpClientFactory.CreateMessageHandlers(options);
                    })
                .ConfigureHttpClient(
                    (sp, c) =>
                    {
                        var options = GetOptions<KubernetesClientOptions>(sp, name);
                        KubernetesHttpClientFactory.ConfigureHttpClient(c, options);
                    });

        // The default KubernetesClient can be resolved directly from DI container
        if (name == string.Empty)
        {
            services.TryAddTransient<KubernetesClient>(sp =>
            {
                var factory = sp.GetRequiredService<IKubernetesClientFactory>();
                return factory.CreateClient(string.Empty);
            });
        }

        return new KubernetesClientBuilder(services, name);
    }

    public static KubernetesClientBuilder AddKubernetesClient(this IServiceCollection services)
    {
        return AddKubernetesClient(services, string.Empty);
    }
}