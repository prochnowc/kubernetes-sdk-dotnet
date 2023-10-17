using System;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models.KubeConfig;

public class ExecCredentialStatus
{
    [JsonPropertyName("expirationTimestamp")]
    [YamlMember(Alias = "expirationTimestamp", ApplyNamingConventions = false)]
    public DateTime? ExpirationTimestamp { get; set; }

    [JsonPropertyName("token")]
    [YamlMember(Alias = "token", ApplyNamingConventions = false)]
    public string? Token { get; set; }

    [JsonPropertyName("clientCertificateData")]
    [YamlMember(Alias = "clientCertificateData ", ApplyNamingConventions = false)]
    public string? ClientCertificateData { get; set; }

    [JsonPropertyName("clientKeyData")]
    [YamlMember(Alias = "clientKeyData ", ApplyNamingConventions = false)]
    public string? ClientKeyData { get; set; }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Token) ||
               (!string.IsNullOrEmpty(ClientCertificateData) && !string.IsNullOrEmpty(ClientKeyData));
    }
}