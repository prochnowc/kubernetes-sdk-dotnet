// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kubernetes.Serialization.Json;

/// <summary>
/// Provides a ISO8601 JSON converter for <see cref="DateTime"/> which uses UTC timezone.
/// </summary>
public sealed class DateTimeConverter : JsonConverter<DateTime>
{
    private static readonly JsonConverter<DateTimeOffset> UtcConverter = new DateTimeOffsetConverter();

    /// <inheritdoc />
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return UtcConverter.Read(ref reader, typeToConvert, options).UtcDateTime;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        UtcConverter.Write(writer, value, options);
    }
}