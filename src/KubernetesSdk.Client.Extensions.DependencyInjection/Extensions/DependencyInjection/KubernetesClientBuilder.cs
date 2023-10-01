using System;
using Microsoft.Extensions.DependencyInjection;

namespace Kubernetes.Client.Extensions.DependencyInjection;

public sealed class KubernetesClientBuilder
{
    public IServiceCollection Services { get; }

    public string Name { get; }

    internal KubernetesClientBuilder(IServiceCollection services, string name)
    {
        Services = services;
        Name = name;
    }

    public KubernetesClientBuilder ConfigureHttpClient(Action<IHttpClientBuilder> configure)
    {
        configure(Services.AddHttpClient(KubernetesClientFactory.GetHttpClientName(Name)));
        return this;
    }

    public KubernetesClientBuilder Configure(Action<KubernetesClientOptions> configure)
    {
        Services.Configure<KubernetesClientBuilderOptions>(o => o.UseDefaultConfig = false);

        Services.AddOptions<KubernetesClientOptions>(Name)
                .Configure(configure);

        return this;
    }

    private KubernetesClientBuilder ConfigureFrom<TProvider>(Action<TProvider>? configure = null)
        where TProvider : class, IKubernetesClientOptionsProvider
    {
        Services.Configure<KubernetesClientBuilderOptions>(o => o.UseDefaultConfig = false);

        Services.AddOptions<KubernetesClientOptions>(Name)
                .Configure<IServiceProvider>(
                    (o, sp) =>
                    {
                        var provider = ActivatorUtilities.CreateInstance<TProvider>(sp);
                        configure?.Invoke(provider);
                        provider.BindOptions(o);
                    });

        return this;
    }

    public KubernetesClientBuilder ConfigureFromKubeConfig(Action<KubeConfigOptionsProvider>? configure = null)
    {
        return ConfigureFrom(configure);
    }

    public KubernetesClientBuilder ConfigureFromKubeConfig(string? configPath, string? context)
    {
        return ConfigureFromKubeConfig(
            p =>
            {
                p.ConfigPath = configPath;
                p.Context = context;
            });
    }

    public KubernetesClientBuilder ConfigureFromCluster()
    {
        return ConfigureFrom<InClusterOptionsProvider>();
    }
}