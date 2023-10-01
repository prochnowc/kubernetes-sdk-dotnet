using Kubernetes.KubeConfig.Models;

namespace Kubernetes.Client.KubeConfig;

public interface IAuthProviderOptionsBinder
{
    /// <summary>
    /// Get the name of the authentication provider.
    /// </summary>
    string ProviderName { get; }

    void BindOptions(KubernetesClientOptions options, AuthProvider provider);
}