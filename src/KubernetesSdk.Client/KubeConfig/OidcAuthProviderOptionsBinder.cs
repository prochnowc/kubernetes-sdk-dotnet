// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Kubernetes.Models.KubeConfig;

namespace Kubernetes.Client.KubeConfig;

/// <summary>
/// Provides a options binder for OIDC authentication.
/// </summary>
public sealed class OidcAuthProviderOptionsBinder : IAuthProviderOptionsBinder
{
    /// <inheritdoc />
    public string ProviderName => "oidc";

    /// <inheritdoc />
    public void BindOptions(KubernetesClientOptions options, AuthProvider provider)
    {
        IDictionary<string, string> config = provider.Config;
        options.AccessToken = config["id-token"];

        if (config.TryGetValue("client-id", out string? clientId)
            && config.TryGetValue("idp-issuer-url", out string? idpIssuerUrl)
            && config.TryGetValue("id-token", out string? idToken)
            && config.TryGetValue("refresh-token", out string? refreshToken))
        {
            config.TryGetValue("client-secret", out string? clientSecret);

            // TODO: options.TokenProvider = new OidcTokenProvider(clientId, clientSecret, idpIssuerUrl, idToken, refreshToken);
        }
    }
}