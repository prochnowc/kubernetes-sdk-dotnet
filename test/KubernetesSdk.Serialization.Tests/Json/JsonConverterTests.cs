// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kubernetes.Serialization.Json;

public abstract class JsonConverterTests<T>
{
    protected abstract JsonConverter<T> CreateConverter();

    protected virtual JsonSerializerOptions CreateOptions()
    {
        return new JsonSerializerOptions()
        {
            Converters = { CreateConverter() },
        };
    }

    protected T? Deserialize(string json)
    {
        return JsonSerializer.Deserialize<T>(json, CreateOptions());
    }

    protected string Serialize(T value)
    {
        return JsonSerializer.Serialize(value, CreateOptions());
    }
}