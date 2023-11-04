using System.Collections.Generic;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models.KubeConfig;

/// <summary>
/// Represents a tuple of references to a cluster (how do I communicate with a kubernetes cluster),
/// a user (how do I identify myself), and a namespace (what subset of resources do I want to work with).
/// </summary>
public class ContextDetails
{
    /// <summary>
    /// Gets or sets the name of the cluster for this context.
    /// </summary>
    [JsonPropertyName("cluster")]
    [YamlMember(Alias = "cluster", ApplyNamingConventions = false)]
    public string? Cluster { get; set; }

    /// <summary>
    /// Gets or sets the name of the user for this context.
    /// </summary>
    [JsonPropertyName("user")]
    [YamlMember(Alias = "user", ApplyNamingConventions = false)]
    public string? User { get; set; }

    /// <summary>
    /// Gets or sets the default namespace to use on unspecified requests.
    /// </summary>
    [JsonPropertyName("namespace")]
    [YamlMember(Alias = "namespace", ApplyNamingConventions = false)]
    public string? Namespace { get; set; }

    /// <summary>
    /// Gets or sets additional information. This is useful for extenders so that reads and writes don't clobber unknown fields.
    /// </summary>
    [JsonPropertyName("extensions")]
    [YamlMember(Alias = "extensions", ApplyNamingConventions = false)]
    public List<NamedExtension> Extensions { get; set; } = new ();
}