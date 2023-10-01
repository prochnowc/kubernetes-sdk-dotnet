using System;
using Kubernetes.Serialization.Json;
using Kubernetes.Serialization.Yaml;
using Microsoft.Extensions.DependencyInjection;

namespace Kubernetes.Client.Extensions.DependencyInjection;

public sealed class KubernetesSerializerBuilder
{
    public IServiceCollection Services { get; }

    public KubernetesSerializerBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public KubernetesSerializerBuilder ConfigureJson(Action<KubernetesJsonOptions> configure)
    {
        Services.Configure(configure);
        return this;
    }

    public KubernetesSerializerBuilder ConfigureYaml(Action<KubernetesYamlOptions> configure)
    {
        Services.Configure(configure);
        return this;
    }
}