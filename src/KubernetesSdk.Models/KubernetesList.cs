// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models;

/// <summary>
/// Represents a list of custom Kubernetes objects.
/// </summary>
/// <typeparam name="T">The type of the Kubernetes object.</typeparam>
public sealed class KubernetesList<T> : IKubernetesList<T>
    where T : IKubernetesObject
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
    [JsonPropertyName("metadata")]
    [YamlMember(Alias = "metadata", ApplyNamingConventions = false)]
    public V1ListMeta Metadata { get; set; }

    /// <inheritdoc />
    [JsonPropertyName("items")]
    [YamlMember(Alias = "items", ApplyNamingConventions = false)]
    public List<T> Items { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesList{T}"/> class.
    /// </summary>
    public KubernetesList()
    {
        Metadata = new V1ListMeta();
        Items = new List<T>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesList{T}"/> class.
    /// </summary>
    /// <param name="items">The items in the list.</param>
    /// <param name="apiVersion">The API version of the list.</param>
    /// <param name="kind">The kind of the list.</param>
    /// <param name="metadata">The <see cref="V1ListMeta"/>.</param>
    public KubernetesList(
        V1ListMeta metadata,
        List<T> items,
        string? apiVersion = default,
        string? kind = default)
    {
        Items = items;
        ApiVersion = apiVersion;
        Kind = kind;
        Metadata = metadata;
    }
}