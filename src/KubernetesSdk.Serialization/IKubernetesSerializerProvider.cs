namespace Kubernetes.Serialization;

/// <summary>
/// Represents a provider for <see cref="IKubernetesSerializer"/> instances.
/// </summary>
public interface IKubernetesSerializerProvider
{
    /// <summary>
    /// Gets the content type of the provided <see cref="IKubernetesSerializer"/>.
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Creates an instance of the serializer.
    /// </summary>
    /// <returns>An instance of the serializer.</returns>
    IKubernetesSerializer CreateSerializer();
}