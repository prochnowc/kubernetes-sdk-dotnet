// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models.KubeConfig;

/// <summary>
/// External credential config model.
/// </summary>
public class ExternalCredential
{
    /// <summary>
    /// Gets or sets the API version used by the external credential process.
    /// </summary>
    [JsonPropertyName("apiVersion")]
    [YamlMember(Alias = "apiVersion", ApplyNamingConventions = false)]
    public string? ApiVersion { get; set; }

    /// <summary>
    /// Gets or sets the command to execute. Required.
    /// </summary>
    [JsonPropertyName("command")]
    [YamlMember(Alias = "command", ApplyNamingConventions = false)]
    public string? Command { get; set; }

    /// <summary>
    /// Gets or sets the environment variables to set when executing the plugin. Optional.
    /// </summary>
    [JsonPropertyName("env")]
    [YamlMember(Alias = "env", ApplyNamingConventions = false)]
    public List<Dictionary<string, string>> EnvironmentVariables { get; set; } = new ();

    /// <summary>
    /// Gets or sets the arguments to pass when executing the plugin. Optional.
    /// </summary>
    [JsonPropertyName("args")]
    [YamlMember(Alias = "args", ApplyNamingConventions = false)]
    public List<string> Arguments { get; set; } = new ();

    /// <summary>
    /// Gets or sets the text shown to the user when the executable doesn't seem to be present. Optional.
    /// </summary>
    [JsonPropertyName("installHint")]
    [YamlMember(Alias = "installHint", ApplyNamingConventions = false)]
    public string? InstallHint { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not to provide cluster information to this exec plugin as a part of
    /// the KUBERNETES_EXEC_INFO environment variable. Optional.
    /// </summary>
    [JsonPropertyName("provideClusterInfo")]
    [YamlMember(Alias = "provideClusterInfo", ApplyNamingConventions = false)]
    public bool ProvideClusterInfo { get; set; }
}