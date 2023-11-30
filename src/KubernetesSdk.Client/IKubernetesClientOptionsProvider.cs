// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

namespace Kubernetes.Client;

/// <summary>
/// Represents a provider for setting up <see cref="KubernetesClientOptions"/>.
/// </summary>
public interface IKubernetesClientOptionsProvider
{
    /// <summary>
    /// Creates a new instance of <see cref="KubernetesClientOptions"/> and populates it.
    /// </summary>
    /// <returns>The <see cref="KubernetesClientOptions"/>.</returns>
    KubernetesClientOptions CreateOptions();

    /// <summary>
    /// Binds options to the provided <see cref="KubernetesClientOptions"/> instance.
    /// </summary>
    /// <param name="options">The <see cref="KubernetesClientOptions"/> instance to bind to.</param>
    void BindOptions(KubernetesClientOptions options);
}