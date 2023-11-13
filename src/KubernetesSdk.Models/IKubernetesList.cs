// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System.Collections.Generic;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models;

/// <summary>
/// Represents a list of Kubernetes objects.
/// </summary>
/// <typeparam name="T">The type of the Kubernetes object.</typeparam>
public interface IKubernetesList<T> : IKubernetesObject<V1ListMeta>
    where T : IKubernetesObject
{
    /// <summary>
    /// Gets or sets the list of Kubernetes objects.
    /// </summary>
    [JsonPropertyName("items")]
    [YamlMember(Alias = "items", ApplyNamingConventions = false)]
    List<T> Items { get; set; }
}