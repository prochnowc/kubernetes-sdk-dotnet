using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Kubernetes.Client.Extensions.Configuration;

public class JsonConfigurationLoader : KubernetesConfigurationLoader
{
    protected override IConfigurationProvider LoadCore(Stream stream)
    {
        return new JsonStreamConfigurationProvider(
            new JsonStreamConfigurationSource
            {
                Stream = stream,
            });
    }
}
