using Kubernetes.Models;

namespace Kubernetes.Serialization.Yaml;

/// <summary>
/// Provides tests for the <see cref="KubernetesYamlSerializer"/>.
/// </summary>
public class KubernetesYamlSerializerTests : KubernetesSerializerTests<V1Deployment>
{
    protected override string Content => YamlContent.Deployment;

    protected override string NullContent => string.Empty;

    protected override IKubernetesSerializer CreateSerializer() => new KubernetesYamlSerializer(new KubernetesYamlOptions());
}