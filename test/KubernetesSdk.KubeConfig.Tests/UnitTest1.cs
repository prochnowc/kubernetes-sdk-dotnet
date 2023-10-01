using Kubernetes.KubeConfig.Models;
using Kubernetes.Models;
using Kubernetes.Serialization;
using Kubernetes.Serialization.Json;
using Kubernetes.Serialization.Yaml;

namespace Kubernetes.KubeConfig;

public class KubeConfigLoaderTests
{
    [Fact]
    public async Task LoadAsync()
    {
        var loader = new KubeConfigLoader();
        V1Config config = await loader.LoadAsync();
    }
}