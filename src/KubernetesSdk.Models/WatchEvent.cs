// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models;

/// <summary>
/// Represents a Kubernetes object event.
/// </summary>
/// <typeparam name="T">The type of the Kubernetes object.</typeparam>
public class WatchEvent<T>
    where T : IKubernetesObject
{
    /// <summary>
    /// Gets or sets the type of the event.
    /// </summary>
    [JsonPropertyName("type")]
    [YamlMember(Alias = "type", ApplyNamingConventions = false)]
    public WatchEventType Type { get; set; }

    /// <summary>
    /// Gets or sets the Kubernetes object that raised the event.
    /// </summary>
    [JsonPropertyName("object")]
    [YamlMember(Alias = "object", ApplyNamingConventions = false)]
    public T? Object { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WatchEvent{T}"/> class.
    /// </summary>
    public WatchEvent()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WatchEvent{T}"/> class.
    /// </summary>
    /// <param name="type">The type of the event.</param>
    /// <param name="object">The Kubernetes object that raised the event.</param>
    public WatchEvent(WatchEventType type, T? @object)
    {
        Type = type;
        Object = @object;
    }
}