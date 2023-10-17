using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models.KubeConfig;

/// <summary>
/// Relates nicknames to auth information.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the auth information.
    /// </summary>
    [JsonPropertyName("user")]
    [YamlMember(Alias = "user", ApplyNamingConventions = false)]
    public UserCredentials? UserCredentials { get; set; }

    /// <summary>
    /// Gets or sets the nickname for this auth information.
    /// </summary>
    [JsonPropertyName("name")]
    [YamlMember(Alias = "name", ApplyNamingConventions = false)]
    public string Name { get; set; } = null!;
}