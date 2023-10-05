using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Kubernetes.KubeConfig.Models;

[YamlSerializable]
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
    public List<Dictionary<string, string>> EnvironmentVariables { get; set; } = new ();

    /// <summary>
    /// Arguments to pass when executing the plugin. Optional.
    /// </summary>
    [YamlMember(Alias = "args")]
    public List<string> Arguments { get; set; } = new ();

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