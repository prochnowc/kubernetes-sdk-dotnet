using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Kubernetes.KubeConfig.Models
{
    public class ExternalCredential
    {
        [YamlMember(Alias = "apiVersion")]
        public string? ApiVersion { get; set; }

        /// <summary>
        /// The command to execute. Required.
        /// </summary>
        [YamlMember(Alias = "command")]
        public string? Command { get; set; }

        /// <summary>
        /// Environment variables to set when executing the plugin. Optional.
        /// </summary>
        [YamlMember(Alias = "env")]
        public IList<IDictionary<string, string>> EnvironmentVariables { get; set; } =
            new List<IDictionary<string, string>>();

        /// <summary>
        /// Arguments to pass when executing the plugin. Optional.
        /// </summary>
        [YamlMember(Alias = "args")]
        public IList<string> Arguments { get; set; } = new List<string>();

        /// <summary>
        /// Text shown to the user when the executable doesn't seem to be present. Optional.
        /// </summary>
        [YamlMember(Alias = "installHint")]
        public string? InstallHint { get; set; }

        /// <summary>
        /// Whether or not to provide cluster information to this exec plugin as a part of
        /// the KUBERNETES_EXEC_INFO environment variable. Optional.
        /// </summary>
        [YamlMember(Alias = "provideClusterInfo")]
        public bool ProvideClusterInfo { get; set; }
    }
}
