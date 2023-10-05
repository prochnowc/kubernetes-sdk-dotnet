using System.Runtime.CompilerServices;
using Kubernetes.Serialization.Yaml;

namespace Kubernetes.KubeConfig.Serialization.Yaml;

internal static class YamlSerializerInitialization
{
    [ModuleInitializer]
    public static void Initialize()
    {
        KubernetesYamlOptions.ConfigureDefaults(
            o => o.Contexts.Add(new KubeConfigYamlContext()));
    }
}