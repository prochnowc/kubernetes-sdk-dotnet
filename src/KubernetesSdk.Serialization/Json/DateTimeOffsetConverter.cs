// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kubernetes.Serialization.Json;

/// <summary>
/// Provides a ISO8601 JSON converter for <see cref="DateTimeOffset"/>.
/// </summary>
public sealed class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    private const string SerializeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.ffffffK";
    private const string Iso8601Format = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK";

    /// <inheritdoc />
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string str = reader.GetString() !;
        return DateTimeOffset.ParseExact(
            str,
            new[] { Iso8601Format, SerializeFormat },
            CultureInfo.InvariantCulture,
            DateTimeStyles.None);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(SerializeFormat));
    }
}