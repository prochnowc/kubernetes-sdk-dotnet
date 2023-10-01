using Kubernetes.Models;

namespace Kubernetes.Serialization.Json;

/// <summary>
/// Provides tests for the <see cref="KubernetesJsonSerializer"/>.
/// </summary>
public class KubernetesJsonSerializerTests : KubernetesSerializerTests<V1Deployment>
{
    protected override string Content => JsonContent.Deployment;

    protected override string NullContent => "null";

    protected override IKubernetesSerializer CreateSerializer() => new KubernetesJsonSerializer(new KubernetesJsonOptions());
}