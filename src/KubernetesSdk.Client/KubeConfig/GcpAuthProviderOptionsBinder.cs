using System.Collections.Generic;
using Kubernetes.KubeConfig.Models;

namespace Kubernetes.Client.KubeConfig;

public sealed class GcpAuthProviderOptionsBinder : IAuthProviderOptionsBinder
{
    public string ProviderName => "gcp";

    public void BindOptions(KubernetesClientOptions options, AuthProvider provider)
    {
        IDictionary<string, string> config = provider.Config;

        // TODO: options.TokenProvider = new GcpTokenProvider(config["cmd-path"]);
    }
}