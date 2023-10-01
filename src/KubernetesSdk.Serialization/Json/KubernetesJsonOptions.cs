// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using Kubernetes.Models;

namespace Kubernetes.Serialization.Json;

/// <summary>
/// Provides options for the <see cref="KubernetesJsonSerializer"/>.
/// </summary>
public sealed class KubernetesJsonOptions
{
    private static JsonSerializerOptions CreateDefaultOptions()
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolverChain =
            {
                KubernetesJsonSerializerContext.Default,
            },
            Converters =
            {
                new TimeSpanConverter(),
                new DateTimeConverter(),
                new DateTimeOffsetConverter(),
                new IntstrIntOrStringConverter(),
                new ResourceQuantityConverter(),
                new JsonStringEnumConverter<WatchEventType>(),

                // TODO: new V1Status.V1StatusObjectViewConverter()
            },
        };

        return options;
    }

    internal static KubernetesJsonOptions Default => new ();

    /// <summary>
    /// Gets the <see cref="T:System.Text.Json.JsonSerializerOptions"/>.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; } = CreateDefaultOptions();
}