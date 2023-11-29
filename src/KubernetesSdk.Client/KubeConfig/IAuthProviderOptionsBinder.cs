// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using Kubernetes.Models.KubeConfig;

namespace Kubernetes.Client.KubeConfig;

/// <summary>
/// Represents a options binder for setting up <see cref="KubernetesClientOptions"/>
/// from <see cref="AuthProvider"/>.
/// </summary>
public interface IAuthProviderOptionsBinder
{
    /// <summary>
    /// Gets the name of the authentication provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Binds options to the provided <see cref="KubernetesClientOptions"/> instance.
    /// </summary>
    /// <param name="options">The <see cref="KubernetesClientOptions"/>.</param>
    /// <param name="provider">The <see cref="AuthProvider"/> to bind.</param>
    void BindOptions(KubernetesClientOptions options, AuthProvider provider);
}