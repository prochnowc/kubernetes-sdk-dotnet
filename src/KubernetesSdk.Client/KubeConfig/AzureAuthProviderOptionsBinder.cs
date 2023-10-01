using System;
using System.Collections.Generic;
using System.Globalization;
using Kubernetes.KubeConfig.Models;

namespace Kubernetes.Client.KubeConfig;

public sealed class AzureAuthProviderOptionsBinder : IAuthProviderOptionsBinder
{
    public string ProviderName => "azure";

    public void BindOptions(KubernetesClientOptions options, AuthProvider provider)
    {
        IDictionary<string, string> config = provider.Config;
        if (config.TryGetValue("expires-on", out string? expiresOn))
        {
            DateTimeOffset expires =
                DateTimeOffset.FromUnixTimeSeconds(int.Parse(expiresOn, CultureInfo.InvariantCulture));

            if (expires <= DateTimeOffset.UtcNow)
            {
                string tenantId = config["tenant-id"];
                string clientId = config["client-id"];
                string apiServerId = config["apiserver-id"];
                string refresh = config["refresh-token"];
                string newToken = RenewAzureToken(tenantId, clientId, apiServerId, refresh);
                config["access-token"] = newToken;
            }
        }

        options.AccessToken = config["access-token"];
    }

    private static string RenewAzureToken(string tenantId, string clientId, string apiServerId, string refresh)
    {
        throw new InvalidOperationException("Refresh not supported.");
    }
}