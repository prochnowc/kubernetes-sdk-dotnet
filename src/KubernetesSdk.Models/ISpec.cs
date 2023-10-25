// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file ist derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models;

/// <summary>
/// Represents a Kubernetes object that has a spec.
/// </summary>
/// <typeparam name="T">type of Kubernetes object.</typeparam>
public interface ISpec<T>
{
    /// <summary>
    /// Gets or sets specification of the desired behavior of the entity.
    /// </summary>
    /// <remarks>
    /// More info:
    /// https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#spec-and-status.
    /// </remarks>
    [JsonPropertyName("spec")]
    [YamlMember(Alias = "spec", ApplyNamingConventions = false)]
    T Spec { get; set; }
}