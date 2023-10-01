// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kubernetes.Models;

namespace Kubernetes.Serialization.Json;

/// <summary>
/// Provides a JSON converter for <see cref="IntstrIntOrString"/>.
/// </summary>
public sealed class IntstrIntOrStringConverter : JsonConverter<IntstrIntOrString>
{
    /// <inheritdoc/>
    public override IntstrIntOrString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return new IntstrIntOrString(reader.GetString());
            case JsonTokenType.Number:
                return new IntstrIntOrString(Convert.ToString(reader.GetInt64(), CultureInfo.InvariantCulture));
        }

        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, IntstrIntOrString value, JsonSerializerOptions options)
    {
        string? stringValue = value.Value;

        if (long.TryParse(stringValue, out long intValue))
        {
            writer.WriteNumberValue(intValue);
            return;
        }

        writer.WriteStringValue(stringValue);
    }
}