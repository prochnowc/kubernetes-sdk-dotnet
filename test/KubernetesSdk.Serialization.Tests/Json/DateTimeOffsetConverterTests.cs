using System;
using System.Text.Json.Serialization;
using FluentAssertions;

namespace Kubernetes.Serialization.Json;

public class DateTimeOffsetConverterTests : JsonConverterTests<DateTimeOffset>
{
    private const string Format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.ffffffK";

    protected override JsonConverter<DateTimeOffset> CreateConverter() => new DateTimeOffsetConverter();

    [Fact]
    public void CanSerializeDateTimeOffset()
    {
        DateTimeOffset value = DateTimeOffset.Now;

        string json = Serialize(value);
        json = json.Replace("\\u002B", "+"); // Json serializer escapes +

        json.Should()
            .Be($"\"{value.ToString(Format)}\"");
    }

    [Fact]
    public void CanDeserializeDateTimeOffsetWithFraction()
    {
        DateTimeOffset value = Deserialize("\"2019-09-07T15:50:00.123456Z\"");

        value.Should()
             .Be(new DateTimeOffset(2019, 9, 7, 15, 50, 0, 0, TimeSpan.Zero).AddTicks(1234560));
    }

    [Fact]
    public void CanDeserializeDateTimeOffset()
    {
        DateTimeOffset value = Deserialize("\"2019-09-07T15:50:00Z\"");

        value.Should()
             .Be(new DateTimeOffset(2019, 9, 7, 15, 50, 0, 0, TimeSpan.Zero));
    }
}