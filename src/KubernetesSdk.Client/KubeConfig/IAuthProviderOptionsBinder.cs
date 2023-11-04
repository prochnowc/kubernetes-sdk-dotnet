using Kubernetes.Models.KubeConfig;

namespace Kubernetes.Client.KubeConfig;

public interface IAuthProviderOptionsBinder
{
    /// <summary>
    /// Gets the name of the authentication provider.
    /// </summary>
    string ProviderName { get; }

    void BindOptions(KubernetesClientOptions options, AuthProvider provider);
}