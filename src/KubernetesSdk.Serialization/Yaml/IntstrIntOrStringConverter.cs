// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System;
using Kubernetes.Models;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Kubernetes.Serialization.Yaml;

/// <summary>
/// Provides a YAML converter for <see cref="IntstrIntOrString"/>.
/// </summary>
public sealed class IntstrIntOrStringConverter : IYamlTypeConverter
{
    /// <inheritdoc/>
    public bool Accepts(Type type)
    {
        return type == typeof(IntstrIntOrString);
    }

    /// <inheritdoc/>
    public object? ReadYaml(IParser parser, Type type)
    {
        if (parser.Current is Scalar scalar)
        {
            try
            {
                if (string.IsNullOrEmpty(scalar.Value))
                {
                    return null;
                }

                return new IntstrIntOrString(scalar.Value);
            }
            finally
            {
                parser.MoveNext();
            }
        }

        throw new InvalidOperationException(parser.Current?.ToString());
    }

    /// <inheritdoc/>
    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        var obj = (IntstrIntOrString?)value;
        if (obj?.Value != null)
        {
            emitter.Emit(new Scalar(obj.Value));
        }
    }
}