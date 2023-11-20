namespace Kubernetes.Client;

/// <summary>
/// Represents the factory for creating <see cref="KubernetesClient"/> instances.
/// </summary>
public interface IKubernetesClientFactory
{
    /// <summary>
    /// Creates the <see cref="KubernetesClient"/> with the specified name.
    /// </summary>
    /// <param name="name">The name of the client; Specify <see cref="string.Empty"/> for the default client.</param>
    /// <returns>The <see cref="KubernetesClient"/>.</returns>
    KubernetesClient CreateClient(string name);
}