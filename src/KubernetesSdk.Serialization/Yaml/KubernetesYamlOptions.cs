// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using AppCore.Diagnostics;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
             .WithNamingConvention(CamelCaseNamingConvention.Instance)
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
             .WithNamingConvention(CamelCaseNamingConvention.Instance)
             .WithTypeInspector(i => new JsonAttributesTypeInspector(i))
             .WithTypeConverter(new IntstrIntOrStringConverter())
             .WithTypeConverter(new ByteArrayStringConverter())
             .WithTypeConverter(new ResourceQuantityConverter())
             .WithAttemptingUnquotedStringTypeDeserialization()
             .IgnoreUnmatchedProperties(),
    };

    internal static KubernetesYamlOptions Default => Defaults.Value;

    public static void ConfigureDefaults(Action<KubernetesYamlOptions> configure)
    {
        Defaults.Configure(configure);
    }

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

    public KubernetesYamlOptions()
    {
        Defaults.PopulateValue(this);
    }

    public IList<StaticContext> Contexts { get; } = new List<StaticContext> { new KubernetesYamlSerializerContext() };
}