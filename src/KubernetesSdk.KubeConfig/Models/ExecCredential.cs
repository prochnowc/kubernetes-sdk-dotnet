using System.Text.Json.Serialization;
using Kubernetes.Models;

namespace Kubernetes.KubeConfig.Models;

[KubernetesEntity("client.authentication.k8s.io", "v1", "ExecCredential")]
public class ExecCredential : IKubernetesObject, ISpec<ExecCredentialSpec?>, IStatus<ExecCredentialStatus?>
{
    [JsonPropertyName("apiVersion")]
    public string? ApiVersion { get; set; }

    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    [JsonPropertyName("spec")]
    public ExecCredentialSpec? Spec { get; set; }

    [JsonPropertyName("status")]
    public ExecCredentialStatus? Status { get; set; }

    public ExecCredential(
        string? apiVersion = default,
        string? kind = default,
        ExecCredentialSpec? spec = default,
        ExecCredentialStatus? status = default)
    {
        ApiVersion = apiVersion;
        Kind = kind;
        Spec = spec;
        Status = status;

        Init();
    }

    private void Init()
    {
        ApiVersion ??= "v1";
        Kind = "ExecCredential";
    }
}