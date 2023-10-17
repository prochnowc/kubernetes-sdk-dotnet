using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models.KubeConfig;

public class ExecCredentialSpec
{
    [JsonPropertyName("interactive")]
    [YamlMember(Alias = "interactive", ApplyNamingConventions = false)]
    public bool Interactive { get; set; }

    /* TODO: Cluster info */
}