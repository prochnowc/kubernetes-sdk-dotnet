using System.Collections.Generic;
using System.Runtime.Serialization;
using Kubernetes.Models;
using YamlDotNet.Serialization;

namespace Kubernetes.KubeConfig.Models
{
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
        /// Gets or sets general information to be use for CLI interactions
        /// </summary>
        [YamlMember(Alias = "preferences")]
        public IDictionary<string, object> Preferences { get; set; } = new Dictionary<string, object>();

        [YamlMember(Alias = "apiVersion")]
        public string? ApiVersion { get; set; }

        [YamlMember(Alias = "kind")]
        public string? Kind { get; set; }

        /// <summary>
        /// Gets or sets the name of the context that you would like to use by default.
        /// </summary>
        [YamlMember(Alias = "current-context", ApplyNamingConventions = false)]
        public string? CurrentContext { get; set; }

        /// <summary>
        /// Gets or sets a map of referencable names to context configs.
        /// </summary>
        [YamlMember(Alias = "contexts")]
        public IList<Context> Contexts { get; set; } = new List<Context>();

        /// <summary>
        /// Gets or sets a map of referencable names to cluster configs.
        /// </summary>
        [YamlMember(Alias = "clusters")]
        public IList<Cluster> Clusters { get; set; } = new List<Cluster>();

        /// <summary>
        /// Gets or sets a map of referencable names to user configs
        /// </summary>
        [YamlMember(Alias = "users")]
        public IList<User> Users { get; set; } = new List<User>();

        /// <summary>
        /// Gets or sets additional information. This is useful for extenders so that reads and writes don't clobber unknown fields.
        /// </summary>
        [YamlMember(Alias = "extensions")]
        public IList<NamedExtension> Extensions { get; set; } = new List<NamedExtension>();
    }
}
