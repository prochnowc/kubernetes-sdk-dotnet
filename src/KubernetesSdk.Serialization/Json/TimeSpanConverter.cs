// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace Kubernetes.Serialization.Json;

/// <summary>
/// Provides a ISO8601 JSON converter for <see cref="TimeSpan"/>.
/// </summary>
public sealed class TimeSpanConverter : JsonConverter<TimeSpan>
{
    /// <inheritdoc/>
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string str = reader.GetString() !;
        return XmlConvert.ToTimeSpan(str);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        string iso8601TimeSpanString = XmlConvert.ToString(value); // XmlConvert for TimeSpan uses ISO8601, so delegate serialization to it
        writer.WriteStringValue(iso8601TimeSpanString);
    }
}