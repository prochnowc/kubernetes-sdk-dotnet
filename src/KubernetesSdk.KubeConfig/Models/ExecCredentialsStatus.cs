using System;

namespace Kubernetes.KubeConfig.Models;

public class ExecCredentialsStatus
{
    public DateTime? ExpirationTimestamp { get; set; }

    public string? Token { get; set; }

    public string? ClientCertificateData { get; set; }

    public string? ClientKeyData { get; set; }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Token) ||
               (!string.IsNullOrEmpty(ClientCertificateData) && !string.IsNullOrEmpty(ClientKeyData));
    }
}