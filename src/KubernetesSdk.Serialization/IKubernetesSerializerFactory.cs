using System;

namespace Kubernetes.Serialization;

/// <summary>
/// Represents a factory for <see cref="IKubernetesSerializer" /> instances.
/// </summary>
public interface IKubernetesSerializerFactory
{
    /// <summary>
    /// Creates the serializer for the specified content type.
    /// </summary>
    /// <param name="contentType">The content type to get the serializer for.</param>
    /// <returns>The serializer for the specified content type.</returns>
    /// <exception cref="ArgumentException">No serializer registered for the specified content type.</exception>
    IKubernetesSerializer CreateSerializer(string contentType);
}