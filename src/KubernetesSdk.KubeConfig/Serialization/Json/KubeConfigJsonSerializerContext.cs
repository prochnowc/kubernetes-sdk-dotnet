// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using Kubernetes.KubeConfig.Models;
using Kubernetes.Models;
using Kubernetes.Serialization.Json;

namespace Kubernetes.KubeConfig.Serialization.Json;

/// <summary>
/// Provides metadata for serializing and deserializing KubeConfig objects using JSON.
/// </summary>
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters = new[]
    {
        typeof(TimeSpanConverter),
        typeof(DateTimeConverter),
        typeof(DateTimeOffsetConverter),
        typeof(IntstrIntOrStringConverter),
        typeof(ResourceQuantityConverter),
        typeof(JsonStringEnumConverter<WatchEventType>),
    })
]
[JsonSerializable(typeof(V1Config))]
[JsonSerializable(typeof(ExecCredential))]
public partial class KubeConfigJsonSerializerContext : JsonSerializerContext
{
}
