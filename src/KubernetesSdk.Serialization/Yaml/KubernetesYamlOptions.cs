// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Kubernetes.Serialization.Yaml;

/// <summary>
/// Provides options for the <see cref="KubernetesYamlSerializer"/>.
/// </summary>
public sealed class KubernetesYamlOptions
{
    private static readonly ValueDefaultsInitializer<KubernetesYamlOptions> Defaults = new ();

    private readonly List<Action<StaticSerializerBuilder>> _configureSerializer = new ()
    {
        b => b
             .DisableAliases()
             .WithTypeInspector(i => new JsonAttributesTypeInspector(i))
             .WithTypeConverter(new IntstrIntOrStringConverter())
             .WithTypeConverter(new ByteArrayStringConverter())
             .WithTypeConverter(new ResourceQuantityConverter())
             .WithEventEmitter(e => new StringQuotingEmitter(e))
             .WithEventEmitter(e => new FloatEmitter(e))
             .ConfigureDefaultValuesHandling(
                 DefaultValuesHandling.OmitNull
                 | DefaultValuesHandling.OmitEmptyCollections
                 | DefaultValuesHandling.OmitDefaults),
    };

    private readonly List<Action<StaticDeserializerBuilder>> _configureDeserializer = new ()
    {
        b => b
             .WithTypeInspector(i => new JsonAttributesTypeInspector(i))
             .WithTypeConverter(new IntstrIntOrStringConverter())
             .WithTypeConverter(new ByteArrayStringConverter())
             .WithTypeConverter(new ResourceQuantityConverter())
             .WithAttemptingUnquotedStringTypeDeserialization()
             .IgnoreUnmatchedProperties(),
    };

    internal static KubernetesYamlOptions Default => Defaults.Value;

    /// <summary>
    /// Configures default <see cref="KubernetesYamlOptions"/>.
    /// </summary>
    /// <param name="configure">The delegate used to configure the defaults.</param>
    public static void ConfigureDefaults(Action<KubernetesYamlOptions> configure)
    {
        Defaults.Configure(configure);
    }

    /// <summary>
    /// Configures the YAML serializer.
    /// </summary>
    /// <param name="configureBuilder">The delegate used to configure the YAML serializer.</param>
    public void ConfigureSerializer(Action<StaticSerializerBuilder> configureBuilder)
    {
        Ensure.Arg.NotNull(configureBuilder);
        _configureSerializer.Add(configureBuilder);
    }

    internal ISerializer BuildSerializer()
    {
        var builder = new StaticSerializerBuilder(new YamlContextChain(Contexts));
        foreach (Action<StaticSerializerBuilder> action in _configureSerializer)
        {
            action(builder);
        }

        return builder.Build();
    }

    /// <summary>
    /// Configures the YAML deserializer.
    /// </summary>
    /// <param name="configureBuilder">A delegate used to configure the YAML deserializer.</param>
    public void ConfigureDeserializer(Action<StaticDeserializerBuilder> configureBuilder)
    {
        Ensure.Arg.NotNull(configureBuilder);
        _configureDeserializer.Add(configureBuilder);
    }

    internal IDeserializer BuildDeserializer()
    {
        var builder = new StaticDeserializerBuilder(new YamlContextChain(Contexts));
        foreach (Action<StaticDeserializerBuilder> action in _configureDeserializer)
        {
            action(builder);
        }

        return builder.Build();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesYamlOptions"/> class.
    /// </summary>
    public KubernetesYamlOptions()
    {
        Defaults.PopulateValue(this);
    }

    /// <summary>
    /// Gets the <see cref="IList{T}"/> of <see cref="StaticContext"/>s.
    /// </summary>
    public IList<StaticContext> Contexts { get; } = new List<StaticContext> { new KubernetesYamlSerializerContext() };
}