using System.Collections.Generic;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models.KubeConfig;

/// <summary>
/// Contains information that describes identity information.  This is use to tell the kubernetes cluster who you are.
/// </summary>
public class UserCredentials
{
    /// <summary>
    /// Gets or sets PEM-encoded data from a client cert file for TLS. Overrides <see cref="ClientCertificate"/>.
    /// </summary>
    [JsonPropertyName("client-certificate-data")]
    [YamlMember(Alias = "client-certificate-data", ApplyNamingConventions = false)]
    public string? ClientCertificateData { get; set; }

    /// <summary>
    /// Gets or sets the path to a client cert file for TLS.
    /// </summary>
    [JsonPropertyName("client-certificate")]
    [YamlMember(Alias = "client-certificate", ApplyNamingConventions = false)]
    public string? ClientCertificate { get; set; }

    /// <summary>
    /// Gets or sets PEM-encoded data from a client key file for TLS. Overrides <see cref="ClientKey"/>.
    /// </summary>
    [JsonPropertyName("client-key-data")]
    [YamlMember(Alias = "client-key-data", ApplyNamingConventions = false)]
    public string? ClientKeyData { get; set; }

    /// <summary>
    /// Gets or sets the path to a client key file for TLS.
    /// </summary>
    [JsonPropertyName("client-key")]
    [YamlMember(Alias = "client-key", ApplyNamingConventions = false)]
    public string? ClientKey { get; set; }

    /// <summary>
    /// Gets or sets the bearer token for authentication to the kubernetes cluster.
    /// </summary>
    [JsonPropertyName("token")]
    [YamlMember(Alias = "token", ApplyNamingConventions = false)]
    public string? Token { get; set; }

    /// <summary>
    /// Gets or sets the username to impersonate. The name matches the flag.
    /// </summary>
    [JsonPropertyName("as")]
    [YamlMember(Alias = "as", ApplyNamingConventions = false)]
    public string? Impersonate { get; set; }

    /// <summary>
    /// Gets or sets the groups to impersonate.
    /// </summary>
    [JsonPropertyName("as-groups")]
    [YamlMember(Alias = "as-groups", ApplyNamingConventions = false)]
    public List<string> ImpersonateGroups { get; set; } = new ();

    /// <summary>
    /// Gets or sets additional information for impersonated user.
    /// </summary>
    [JsonPropertyName("as-user-extra")]
    [YamlMember(Alias = "as-user-extra", ApplyNamingConventions = false)]
    public Dictionary<string, string> ImpersonateUserExtra { get; set; } = new ();

    /// <summary>
    /// Gets or sets the username for basic authentication to the kubernetes cluster.
    /// </summary>
    [JsonPropertyName("username")]
    [YamlMember(Alias = "username", ApplyNamingConventions = false)]
    public string? UserName { get; set; }

    /// <summary>
    /// Gets or sets the password for basic authentication to the kubernetes cluster.
    /// </summary>
    [JsonPropertyName("password")]
    [YamlMember(Alias = "password", ApplyNamingConventions = false)]
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets custom authentication plugin for the kubernetes cluster.
    /// </summary>
    [JsonPropertyName("auth-provider")]
    [YamlMember(Alias = "auth-provider", ApplyNamingConventions = false)]
    public AuthProvider? AuthProvider { get; set; }

    /// <summary>
    /// Gets or sets additional information. This is useful for extenders so that reads and writes don't clobber unknown fields.
    /// </summary>
    [JsonPropertyName("extensions")]
    [YamlMember(Alias = "extensions", ApplyNamingConventions = false)]
    public List<NamedExtension> Extensions { get; set; } = new ();

    /// <summary>
    /// Gets or sets external command and its arguments to receive user credentials.
    /// </summary>
    [JsonPropertyName("exec")]
    [YamlMember(Alias = "exec", ApplyNamingConventions = false)]
    public ExternalCredential? Execute { get; set; }
}