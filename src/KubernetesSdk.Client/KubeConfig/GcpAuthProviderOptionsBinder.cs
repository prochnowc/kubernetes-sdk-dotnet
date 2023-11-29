// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Kubernetes.Models.KubeConfig;

namespace Kubernetes.Client.KubeConfig;

/// <summary>
/// Provides a options binder for Google Cloud Platform.
/// </summary>
public sealed class GcpAuthProviderOptionsBinder : IAuthProviderOptionsBinder
{
    /// <inheritdoc />
    public string ProviderName => "gcp";

    /// <inheritdoc />
    public void BindOptions(KubernetesClientOptions options, AuthProvider provider)
    {
        IDictionary<string, string> config = provider.Config;

        // TODO: options.TokenProvider = new GcpTokenProvider(config["cmd-path"]);
    }
}