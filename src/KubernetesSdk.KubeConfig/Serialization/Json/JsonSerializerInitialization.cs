using System.Runtime.CompilerServices;
using Kubernetes.Serialization.Json;

namespace Kubernetes.KubeConfig.Serialization.Json;

internal static class JsonSerializerInitialization
{
    [ModuleInitializer]
    public static void Initialize()
    {
        KubernetesJsonOptions.ConfigureDefaults(
            o => o.JsonSerializerOptions.TypeInfoResolverChain.Add(KubeConfigJsonSerializerContext.Default));
    }
}