using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models.KubeConfig;

/// <summary>
/// Relates nicknames to context information.
/// </summary>
public class Context
{
    /// <summary>
    /// Gets or sets the context information.
    /// </summary>
    [JsonPropertyName("context")]
    [YamlMember(Alias = "context", ApplyNamingConventions = false)]
    public ContextDetails? ContextDetails { get; set; }

    /// <summary>
    /// Gets or sets the nickname for this context.
    /// </summary>
    [JsonPropertyName("name")]
    [YamlMember(Alias = "name", ApplyNamingConventions = false)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets additional information. This is useful for extenders so that reads and writes don't clobber unknown fields.
    /// </summary>
    [JsonPropertyName("extensions")]
    [YamlMember(Alias = "extensions", ApplyNamingConventions = false)]
    public List<NamedExtension> Extensions { get; set; } = new ();

    [Obsolete("This property is not set by the YAML config. Use ContextDetails.Namespace instead.")]
    [JsonPropertyName("namespace")]
    [YamlMember(Alias = "namespace", ApplyNamingConventions = false)]
    public string? Namespace { get; set; }
}