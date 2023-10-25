// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models;

/// <summary>
/// Represents a Kubernetes object.
/// </summary>
/// <remarks>
/// You can use the <see cref="IKubernetesObject"/> if you receive JSON from a Kubernetes API server but
/// are unsure which object the API server is about to return. You can parse the JSON as a <see cref="IKubernetesObject"/>
/// and use the <see cref="ApiVersion"/> and <see cref="Kind"/> properties to get basic metadata about any Kubernetes object.
/// </remarks>
public interface IKubernetesObject
{
    /// <summary>
    /// Gets or sets the API version.
    /// </summary>
    /// <remarks>
    /// ApiVersion defines the versioned schema of this
    /// representation of an object. Servers should convert recognized
    /// schemas to the latest internal value, and may reject unrecognized
    /// values. More info:
    /// https://git.k8s.io/community/contributors/devel/api-conventions.md#resources.
    /// </remarks>
    [JsonPropertyName("apiVersion")]
    [YamlMember(Alias = "apiVersion", ApplyNamingConventions = false)]
    string? ApiVersion { get; set; }

    /// <summary>
    /// Gets or sets the kind.
    /// </summary>
    /// <remarks>
    /// Kind is a string value representing the REST resource
    /// this object represents. Servers may infer this from the endpoint
    /// the client submits requests to. Cannot be updated. In CamelCase.
    /// More info:
    /// https://git.k8s.io/community/contributors/devel/api-conventions.md#types-kinds.
    /// </remarks>
    [JsonPropertyName("kind")]
    [YamlMember(Alias = "kind", ApplyNamingConventions = false)]
    string? Kind { get; set; }
}

/// <summary>
/// Represents a Kubernetes object that has metadata.
/// </summary>
/// <typeparam name="TMetadata">The type of metadata.</typeparam>
public interface IKubernetesObject<TMetadata> : IKubernetesObject, IMetadata<TMetadata>
{
}