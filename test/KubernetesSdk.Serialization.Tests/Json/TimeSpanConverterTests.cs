using System;
using System.Text.Json.Serialization;
using FluentAssertions;

namespace Kubernetes.Serialization.Json;

public class TimeSpanConverterTests : JsonConverterTests<TimeSpan>
{
    protected override JsonConverter<TimeSpan> CreateConverter() => new TimeSpanConverter();

    [Fact]
    public void CanSerializeTimeSpan()
    {
        var value = new TimeSpan(4, 12, 30, 5);

        string json = Serialize(value);
        json.Should()
            .Be("\"P4DT12H30M5S\"");
    }

    [Fact]
    public void CanDeserializeTimeSpan()
    {
        TimeSpan value = Deserialize("\"P4DT12H30M5S\"");

        value.Should()
             .Be(new TimeSpan(4, 12, 30, 5));
    }
}