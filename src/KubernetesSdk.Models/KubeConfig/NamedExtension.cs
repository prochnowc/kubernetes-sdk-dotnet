using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models.KubeConfig;

/// <summary>
/// <see cref="NamedExtension"/> relates nicknames to extension information.
/// </summary>
public class NamedExtension
{
    /// <summary>
    /// Gets or sets the nickname for this extension.
    /// </summary>
    [JsonPropertyName("name")]
    [YamlMember(Alias = "name", ApplyNamingConventions = false)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the extension information.
    /// </summary>
    [JsonPropertyName("extension")]
    [YamlMember(Alias = "extension", ApplyNamingConventions = false)]
    public object? Extension { get; set; }
}