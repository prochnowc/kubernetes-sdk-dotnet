// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models.KubeConfig;

/// <summary>
/// Represents the specification of an external credential process.
/// </summary>
public class ExecCredentialSpec
{
    /// <summary>
    /// Gets or sets a value indicating whether the process is running in interactive mode.
    /// </summary>
    [JsonPropertyName("interactive")]
    [YamlMember(Alias = "interactive", ApplyNamingConventions = false)]
    public bool Interactive { get; set; }

    /* TODO: Cluster info */
}