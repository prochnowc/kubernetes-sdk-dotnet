// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

namespace Kubernetes.Client.Operations;

/// <summary>
/// Provides the base class for operations of the <see cref="KubernetesClient"/>.
/// </summary>
public abstract class KubernetesClientOperations
{
    /// <summary>
    /// Gets the <see cref="KubernetesClient"/>.
    /// </summary>
    protected KubernetesClient Client { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesClientOperations"/> class.
    /// </summary>
    /// <param name="client">The <see cref="KubernetesClient"/>.</param>
    protected KubernetesClientOperations(KubernetesClient client)
    {
        Ensure.Arg.NotNull(client);
        Client = client;
    }
}
