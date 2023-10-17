using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models.KubeConfig;

[KubernetesEntity("client.authentication.k8s.io", "v1", "ExecCredential")]
public class ExecCredential : IKubernetesObject, ISpec<ExecCredentialSpec?>, IStatus<ExecCredentialStatus?>
{
    [JsonPropertyName("apiVersion")]
    [YamlMember(Alias = "apiVersion", ApplyNamingConventions = false)]
    public string? ApiVersion { get; set; }

    [JsonPropertyName("kind")]
    [YamlMember(Alias = "kind", ApplyNamingConventions = false)]
    public string? Kind { get; set; }

    [JsonPropertyName("spec")]
    [YamlMember(Alias = "spec", ApplyNamingConventions = false)]
    public ExecCredentialSpec? Spec { get; set; }

    [JsonPropertyName("status")]
    [YamlMember(Alias = "status", ApplyNamingConventions = false)]
    public ExecCredentialStatus? Status { get; set; }

    public ExecCredential()
    {
        Init();
    }

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