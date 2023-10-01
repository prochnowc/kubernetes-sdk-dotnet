// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using AppCore.Diagnostics;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Kubernetes.Serialization.Yaml;

/// <summary>
/// Provides options for the <see cref="KubernetesYamlSerializer"/>.
/// </summary>
public sealed class KubernetesYamlOptions
{
    private readonly List<Action<SerializerBuilder>> _configureSerializer = new ()
    {
        b => b
             .DisableAliases()
             .EnablePrivateConstructors()
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

    private readonly List<Action<DeserializerBuilder>> _configureDeserializer = new ()
    {
        b => b
             .WithNamingConvention(CamelCaseNamingConvention.Instance)
             .EnablePrivateConstructors()
             .WithTypeInspector(i => new JsonAttributesTypeInspector(i))
             .WithTypeConverter(new IntstrIntOrStringConverter())
             .WithTypeConverter(new ByteArrayStringConverter())
             .WithTypeConverter(new ResourceQuantityConverter())
             .WithAttemptingUnquotedStringTypeDeserialization()
             .IgnoreUnmatchedProperties(),
    };

    internal static KubernetesYamlOptions Default => new ();

    public void ConfigureSerializer(Action<SerializerBuilder> configureBuilder)
    {
        Ensure.Arg.NotNull(configureBuilder);
        _configureSerializer.Add(configureBuilder);
    }

    internal ISerializer BuildSerializer()
    {
        var builder = new SerializerBuilder();
        foreach (Action<SerializerBuilder> action in _configureSerializer)
        {
            action(builder);
        }

        return builder.Build();
    }

    public void ConfigureDeserializer(Action<DeserializerBuilder> configureBuilder)
    {
        Ensure.Arg.NotNull(configureBuilder);
        _configureDeserializer.Add(configureBuilder);
    }

    internal IDeserializer BuildDeserializer()
    {
        var builder = new DeserializerBuilder();
        foreach (Action<DeserializerBuilder> action in _configureDeserializer)
        {
            action(builder);
        }

        return builder.Build();
    }
}