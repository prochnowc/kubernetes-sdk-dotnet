using System.Text.Json.Serialization;
using Kubernetes.Models;

namespace Kubernetes.KubeConfig.Models
{
    public class ExecCredentialsResponse : IKubernetesObject
    {
        [JsonPropertyName("apiVersion")]
        public string? ApiVersion { get; set; }

        [JsonPropertyName("kind")]
        public string? Kind { get; set; }

        [JsonPropertyName("status")]
        public ExecCredentialsStatus? Status { get; set; }
    }
}
