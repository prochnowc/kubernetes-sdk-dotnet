using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models.KubeConfig;

/// <summary>
/// Relates nicknames to cluster information.
/// </summary>
public class Cluster
{
    /// <summary>
    /// Gets or sets the nickname for this Cluster.
    /// </summary>
    [JsonPropertyName("name")]
    [YamlMember(Alias = "name", ApplyNamingConventions = false)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the cluster information.
    /// </summary>
    [JsonPropertyName("cluster")]
    [YamlMember(Alias = "cluster", ApplyNamingConventions = false)]
    public ClusterEndpoint? ClusterEndpoint { get; set; }
}