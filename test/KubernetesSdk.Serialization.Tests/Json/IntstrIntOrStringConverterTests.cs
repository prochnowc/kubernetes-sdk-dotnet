// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using FluentAssertions;
using Kubernetes.Models;

namespace Kubernetes.Serialization.Json;

public class IntstrIntOrStringConverterTests : JsonConverterTests<IntstrIntOrString>
{
    protected override JsonConverter<IntstrIntOrString> CreateConverter() => new IntstrIntOrStringConverter();

    [Fact]
    public void CanSerializeInt()
    {
        string json = Serialize(new IntstrIntOrString("123"));
        json.Should()
            .Be("123");
    }

    [Fact]
    public void CanSerializeString()
    {
        string json = Serialize(new IntstrIntOrString("123%"));
        json.Should()
            .Be("\"123%\"");
    }

    [Fact]
    public void CanDeserializeInt()
    {
        IntstrIntOrString value = Deserialize("123") !;
        value.Value.Should()
             .Be("123");
    }

    [Fact]
    public void CanDeserializeString()
    {
        IntstrIntOrString value = Deserialize("\"123\"") !;
        value.Value.Should()
             .Be("123");
    }
}