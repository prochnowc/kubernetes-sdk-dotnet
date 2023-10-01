// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Kubernetes.Serialization.Yaml;

/// <summary>
/// Provides a YAML converter for <see cref="byte"/> arrays.
/// </summary>
public sealed class ByteArrayStringConverter : IYamlTypeConverter
{
    /// <inheritdoc />
    public bool Accepts(Type type)
    {
        return type == typeof(byte[]);
    }

    /// <inheritdoc />
    public object? ReadYaml(IParser parser, Type type)
    {
        if (parser.Current is Scalar scalar)
        {
            try
            {
                return string.IsNullOrEmpty(scalar.Value)
                    ? null
                    : Encoding.UTF8.GetBytes(scalar.Value);
            }
            finally
            {
                parser.MoveNext();
            }
        }

        throw new InvalidOperationException(parser.Current?.ToString());
    }

    /// <inheritdoc />
    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (value is byte[] obj)
        {
            emitter.Emit(new Scalar(Encoding.UTF8.GetString(obj)));
        }
    }
}