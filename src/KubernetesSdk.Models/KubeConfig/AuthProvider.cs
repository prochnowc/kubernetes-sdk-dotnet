using System.Collections.Generic;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models.KubeConfig;

/// <summary>
/// Contains information that describes identity information.  This is use to tell the kubernetes cluster who you are.
/// </summary>
public class AuthProvider
{
    /// <summary>
    /// Gets or sets the nickname for this auth provider.
    /// </summary>
    [JsonPropertyName("name")]
    [YamlMember(Alias = "name", ApplyNamingConventions = false)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the configuration for this auth provider.
    /// </summary>
    [JsonPropertyName("config")]
    [YamlMember(Alias = "config", ApplyNamingConventions = false)]
    public Dictionary<string, string> Config { get; set; } = new ();
}