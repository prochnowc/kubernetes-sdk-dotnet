using System.Collections.Generic;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models.KubeConfig;

/// <summary>
/// kubeconfig configuration model. Holds the information needed to build connect to remote
/// Kubernetes clusters as a given user.
/// </summary>
/// <remarks>
/// Should be kept in sync with https://github.com/kubernetes/kubernetes/blob/master/staging/src/k8s.io/client-go/tools/clientcmd/api/v1/types.go
/// Should update MergeKubeConfig in KubernetesClientConfiguration.ConfigFile.cs if updated.
/// </remarks>
[KubernetesEntity("", "v1", "Config")]
public class V1Config : IKubernetesObject
{
    /// <summary>
    /// Gets or sets general information to be use for CLI interactions.
    /// </summary>
    [JsonPropertyName("preferences")]
    [YamlMember(Alias = "preferences", ApplyNamingConventions = false)]
    public Dictionary<string, object> Preferences { get; set; } = new ();

    /// <inheritdoc />
    [JsonPropertyName("apiVersion")]
    [YamlMember(Alias = "apiVersion", ApplyNamingConventions = false)]
    public string? ApiVersion { get; set; }

    /// <inheritdoc />
    [JsonPropertyName("kind")]
    [YamlMember(Alias = "kind", ApplyNamingConventions = false)]
    public string? Kind { get; set; }

    /// <summary>
    /// Gets or sets the name of the context that you would like to use by default.
    /// </summary>
    [JsonPropertyName("current-context")]
    [YamlMember(Alias = "current-context", ApplyNamingConventions = false)]
    public string? CurrentContext { get; set; }

    /// <summary>
    /// Gets or sets a map of referencable names to context configs.
    /// </summary>
    [JsonPropertyName("contexts")]
    [YamlMember(Alias = "contexts", ApplyNamingConventions = false)]
    public List<Context> Contexts { get; set; } = new ();

    /// <summary>
    /// Gets or sets a map of referencable names to cluster configs.
    /// </summary>
    [JsonPropertyName("clusters")]
    [YamlMember(Alias = "clusters", ApplyNamingConventions = false)]
    public List<Cluster> Clusters { get; set; } = new ();

    /// <summary>
    /// Gets or sets a map of referencable names to user configs.
    /// </summary>
    [JsonPropertyName("users")]
    [YamlMember(Alias = "users", ApplyNamingConventions = false)]
    public List<User> Users { get; set; } = new ();

    /// <summary>
    /// Gets or sets additional information. This is useful for extenders so that reads and writes don't clobber unknown fields.
    /// </summary>
    [JsonPropertyName("extensions")]
    [YamlMember(Alias = "extensions", ApplyNamingConventions = false)]
    public List<NamedExtension> Extensions { get; set; } = new ();

    /// <summary>
    /// Initializes a new instance of the <see cref="V1Config"/> class.
    /// </summary>
    public V1Config()
    {
        Init();
    }

    private void Init()
    {
        ApiVersion ??= "v1";
        Kind = "Config";
    }
}