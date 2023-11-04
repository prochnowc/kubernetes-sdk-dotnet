// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models.KubeConfig;

/// <summary>
/// Credential model used to acquire authentication credentials using an external process.
/// </summary>
[KubernetesEntity("client.authentication.k8s.io", "v1", "ExecCredential")]
public class ExecCredential : IKubernetesObject, ISpec<ExecCredentialSpec?>, IStatus<ExecCredentialStatus?>
{
    /// <inheritdoc />
    [JsonPropertyName("apiVersion")]
    [YamlMember(Alias = "apiVersion", ApplyNamingConventions = false)]
    public string? ApiVersion { get; set; }

    /// <inheritdoc />
    [JsonPropertyName("kind")]
    [YamlMember(Alias = "kind", ApplyNamingConventions = false)]
    public string? Kind { get; set; }

    /// <inheritdoc />
    [JsonPropertyName("spec")]
    [YamlMember(Alias = "spec", ApplyNamingConventions = false)]
    public ExecCredentialSpec? Spec { get; set; }

    /// <inheritdoc />
    [JsonPropertyName("status")]
    [YamlMember(Alias = "status", ApplyNamingConventions = false)]
    public ExecCredentialStatus? Status { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecCredential"/> class.
    /// </summary>
    public ExecCredential()
    {
        Init();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecCredential"/> class.
    /// </summary>
    /// <param name="apiVersion">The API version.</param>
    /// <param name="kind">The Kubernetes schema kind.</param>
    /// <param name="spec">The object spec.</param>
    /// <param name="status">The object status.</param>
    public ExecCredential(
        string? apiVersion = default,
        string? kind = default,
        ExecCredentialSpec? spec = default,
        ExecCredentialStatus? status = default)
    {
        ApiVersion = apiVersion;
        Kind = kind;
        Spec = spec;
        Status = status;

        Init();
    }

    private void Init()
    {
        ApiVersion ??= "v1";
        Kind = "ExecCredential";
    }
}