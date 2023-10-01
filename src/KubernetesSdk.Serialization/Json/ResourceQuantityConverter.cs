// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kubernetes.Models;

namespace Kubernetes.Serialization.Json;

/// <summary>
/// Provides a JSON converter for <see cref="ResourceQuantity"/>.
/// </summary>
public sealed class ResourceQuantityConverter : JsonConverter<ResourceQuantity>
{
    /// <inheritdoc/>
    public override ResourceQuantity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new ResourceQuantity(reader.GetString());
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, ResourceQuantity value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}