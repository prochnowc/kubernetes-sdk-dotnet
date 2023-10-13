using YamlDotNet.Serialization;

namespace Kubernetes.KubeConfig.Models;

/// <summary>
/// <see cref="NamedExtension"/> relates nicknames to extension information
/// </summary>
[YamlSerializable]
public class NamedExtension
{
    /// <summary>
    /// Gets or sets the nickname for this extension.
    /// </summary>
    [YamlMember(Alias = "name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the extension information.
    /// </summary>
    [YamlMember(Alias = "extension")]
    public object? Extension { get; set; }
}