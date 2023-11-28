// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kubernetes.Models;

namespace Kubernetes.Serialization.Json;

/// <summary>
/// Provides options for the <see cref="KubernetesJsonSerializer"/>.
/// </summary>
public sealed class KubernetesJsonOptions
{
    private static readonly ValueDefaultsInitializer<KubernetesJsonOptions> Defaults = new ();

    private static JsonSerializerOptions CreateDefaultJsonSerializerOptions()
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

    /// <summary>
    /// Configures default <see cref="KubernetesJsonOptions"/>.
    /// </summary>
    /// <param name="configure">The delegate used to configure the defaults.</param>
    public static void ConfigureDefaults(Action<KubernetesJsonOptions> configure)
    {
        Defaults.Configure(configure);
    }

    internal static KubernetesJsonOptions Default => Defaults.Value;

    /// <summary>
    /// Gets the <see cref="T:System.Text.Json.JsonSerializerOptions"/>.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; } = CreateDefaultJsonSerializerOptions();

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesJsonOptions"/> class.
    /// </summary>
    public KubernetesJsonOptions()
    {
        Defaults.PopulateValue(this);
    }
}