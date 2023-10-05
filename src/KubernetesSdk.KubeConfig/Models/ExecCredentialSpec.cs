using System.Text.Json.Serialization;

namespace Kubernetes.KubeConfig.Models;

public class ExecCredentialSpec
{
    [JsonPropertyName("interactive")]
    public bool Interactive { get; set; }

    /* TODO: Cluster info */

    public ExecCredentialSpec(bool interactive)
    {
        Interactive = interactive;
    }
}