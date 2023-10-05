using YamlDotNet.Serialization;

namespace Kubernetes.KubeConfig.Models;

/// <summary>
/// Relates nicknames to auth information.
/// </summary>
[YamlSerializable]
public class User
{
    /// <summary>
    /// Gets or sets the auth information.
    /// </summary>
    [YamlMember(Alias = "user")]
    public UserCredentials? UserCredentials { get; set; }

    /// <summary>
    /// Gets or sets the nickname for this auth information.
    /// </summary>
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = null!;
}