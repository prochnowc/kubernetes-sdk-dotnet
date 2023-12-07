// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Kubernetes.Models.KubeConfig;

namespace Kubernetes.Client.KubeConfig;

/// <summary>
/// Provides a options binder for Azure authentication.
/// </summary>
public sealed class AzureAuthProviderOptionsBinder : IAuthProviderOptionsBinder
{
    /// <inheritdoc />
    public string ProviderName => "azure";

    /// <inheritdoc />
    public void BindOptions(KubernetesClientOptions options, AuthProvider provider)
    {
        IDictionary<string, string> config = provider.Config;
        if (config.TryGetValue("expires-on", out string? expiresOn))
        {
            DateTimeOffset expires =
                DateTimeOffset.FromUnixTimeSeconds(int.Parse(expiresOn, CultureInfo.InvariantCulture));

            if (expires <= TimeProvider.UtcNow)
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