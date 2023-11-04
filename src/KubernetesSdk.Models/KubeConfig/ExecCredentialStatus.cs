// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models.KubeConfig;

/// <summary>
/// Represents the status of a external credential process.
/// </summary>
public class ExecCredentialStatus
{
    /// <summary>
    /// Gets or sets a value indicating when the authentication credential will expire.
    /// </summary>
    [JsonPropertyName("expirationTimestamp")]
    [YamlMember(Alias = "expirationTimestamp", ApplyNamingConventions = false)]
    public DateTime? ExpirationTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the access token used to authenticate.
    /// </summary>
    [JsonPropertyName("token")]
    [YamlMember(Alias = "token", ApplyNamingConventions = false)]
    public string? Token { get; set; }

    /// <summary>
    /// Gets or sets the client certificate used to authenticate.
    /// </summary>
    [JsonPropertyName("clientCertificateData")]
    [YamlMember(Alias = "clientCertificateData ", ApplyNamingConventions = false)]
    public string? ClientCertificateData { get; set; }

    /// <summary>
    /// Gets or sets the client certificate key used to authenticate.
    /// </summary>
    [JsonPropertyName("clientKeyData")]
    [YamlMember(Alias = "clientKeyData ", ApplyNamingConventions = false)]
    public string? ClientKeyData { get; set; }

    /// <summary>
    /// Gets a value indicating whether the external credential is valid.
    /// </summary>
    /// <returns><c>true</c> if the credential is valid; otherwise, <c>false</c>.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Token) ||
               (!string.IsNullOrEmpty(ClientCertificateData) && !string.IsNullOrEmpty(ClientKeyData));
    }
}