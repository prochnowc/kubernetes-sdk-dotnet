using System;
using Microsoft.Extensions.Configuration;

namespace Kubernetes.Client.Extensions.Configuration;

public static class KubernetesConfigurationBuilderExtension
{
    public static IConfigurationBuilder AddConfigMap(
        this IConfigurationBuilder builder,
        string @namespace,
        string name,
        Action<ConfigMapConfigurationSource>? configure)
    {
        var source = new ConfigMapConfigurationSource()
        {
            Namespace = @namespace,
            Name = name,
        };

        return builder.Add(source);
    }
}