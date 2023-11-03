using System.Collections.Generic;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models.KubeConfig;

/// <summary>
/// Contains information about how to communicate with a Kubernetes cluster.
/// </summary>
public class ClusterEndpoint
{
    /// <summary>
    /// Gets or sets the path to a cert file for the certificate authority.
    /// </summary>
    [JsonPropertyName("certificate-authority")]
    [YamlMember(Alias = "certificate-authority", ApplyNamingConventions = false)]
    public string? CertificateAuthority { get; set; }

    /// <summary>
    /// Gets or sets the PEM-encoded certificate authority certificates. Overrides <see cref="CertificateAuthority"/>.
    /// </summary>
    [JsonPropertyName("certificate-authority-data")]
    [YamlMember(Alias = "certificate-authority-data", ApplyNamingConventions = false)]
    public string? CertificateAuthorityData { get; set; }

    /// <summary>
    /// Gets or sets the address of the kubernetes cluster (https://hostname:port).
    /// </summary>
    [JsonPropertyName("server")]
    [YamlMember(Alias = "server", ApplyNamingConventions = false)]
    public string? Server { get; set; }

    /// <summary>
    /// Gets or sets a value to override the TLS server name.
    /// </summary>
    [JsonPropertyName("tls-server-name")]
    [YamlMember(Alias = "tls-server-name", ApplyNamingConventions = false)]
    public string? TlsServerName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to skip the validity check for the server's certificate.
    /// This will make your HTTPS connections insecure.
    /// </summary>
    [JsonPropertyName("insecure-skip-tls-verify")]
    [YamlMember(Alias = "insecure-skip-tls-verify", ApplyNamingConventions = false)]
    public bool SkipTlsVerify { get; set; }

    /// <summary>
    /// Gets or sets additional information. This is useful for extenders so that reads and writes don't clobber unknown fields.
    /// </summary>
    [JsonPropertyName("extensions")]
    [YamlMember(Alias = "extensions", ApplyNamingConventions = false)]
    public List<NamedExtension> Extensions { get; set; } = new ();
}