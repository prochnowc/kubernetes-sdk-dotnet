// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models;

/// <summary>
/// Represents a Kubernetes object that exposes status.
/// </summary>
/// <typeparam name="T">The type of status object.</typeparam>
public interface IStatus<T>
{
    /// <summary>
    /// Gets or sets most recently observed status of the object.
    /// </summary>
    /// <remarks>
    /// This data may not be up to date. Populated by the system. Read-only. More info:
    /// https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#spec-and-status.
    /// </remarks>
    [JsonPropertyName("status")]
    [YamlMember(Alias = "status", ApplyNamingConventions = false)]
    T Status { get; set; }
}