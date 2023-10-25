// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Kubernetes.Models;

/// <summary>
/// Represents a Kubernetes object that exposes metadata.
/// </summary>
/// <typeparam name="T">Type of metadata exposed. Usually this will be either
/// <see cref="V1ListMeta"/> for lists or <see cref="V1ObjectMeta"/> for objects.
/// </typeparam>
public interface IMetadata<T>
{
    /// <summary>
    /// Gets or sets standard object's metadata.
    /// </summary>
    /// <remarks>
    /// More info:
    /// https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#metadata.
    /// </remarks>
    [JsonPropertyName("metadata")]
    [YamlMember(Alias = "metadata", ApplyNamingConventions = false)]
    T Metadata { get; set; }
}
