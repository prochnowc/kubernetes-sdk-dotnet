using System;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Models;
using Microsoft.Extensions.Configuration;

namespace Kubernetes.Client.Extensions.Configuration;

public class ConfigMapConfigurationProvider : ConfigurationProvider, IDisposable
{
    private readonly ConfigMapConfigurationSource _source;

    public ConfigMapConfigurationProvider(ConfigMapConfigurationSource source)
    {
        _source = source;
    }

    public override void Load()
    {
    }

    public async Task Load(KubernetesClient client, CancellationToken cancellationToken)
    {
        V1ConfigMap configMap =
            await client.CoreV1()
                        .ReadNamespacedConfigMapAsync(_source.Name, _source.Namespace, cancellationToken: cancellationToken);

        OnReload();
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}