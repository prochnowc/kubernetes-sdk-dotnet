using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Serialization;

/// <summary>
/// Represents a serializer and deserializer for Kubernetes objects.
/// </summary>
public interface IKubernetesSerializer
{
    Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default);

    T? Deserialize<T>(Stream stream);

    T? Deserialize<T>(ReadOnlySpan<char> content);

    Task SerializeAsync<T>(Stream stream, T value, CancellationToken cancellationToken = default);

    void Serialize<T>(Stream stream, T value);

    string Serialize<T>(T value);
}