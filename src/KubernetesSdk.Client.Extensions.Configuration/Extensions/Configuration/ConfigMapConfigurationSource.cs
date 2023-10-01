using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Kubernetes.Client.Extensions.Configuration;

public class ConfigMapConfigurationSource : IConfigurationSource
{
    public string? Name { get; set; }

    public string? Namespace { get; set; }

    public IDictionary<string, IKubernetesConfigurationLoader> Sources { get; } =
        new Dictionary<string, IKubernetesConfigurationLoader>();

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new ConfigMapConfigurationProvider(this);
    }
}