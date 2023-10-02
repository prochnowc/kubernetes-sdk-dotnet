using System.Threading.Tasks;
using Kubernetes.KubeConfig.Models;

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