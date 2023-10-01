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