// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Models;

namespace Kubernetes.Client.LeaderElection;

/// <summary>
/// Provides a annotation lock implementation on a <see cref="V1Endpoints"/>.
/// </summary>
public sealed class EndpointsLock : KubernetesObjectAnnotationLock<V1Endpoints>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointsLock"/> class.
    /// </summary>
    /// <param name="namespace">The namespace of the object.</param>
    /// <param name="name">The name of the object.</param>
    /// <param name="identity">The identity of the lock owner.</param>
    public EndpointsLock(string @namespace, string name, string identity)
        : base(@namespace, name, identity)
    {
    }

    /// <inheritdoc />
    protected override async Task<V1Endpoints> ReadObjectAsync(
        KubernetesClient client,
        CancellationToken cancellationToken)
    {
        return await client.CoreV1()
                           .ReadNamespacedEndpointsAsync(Name, Namespace, cancellationToken: cancellationToken)
                           .ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override async Task<V1Endpoints> CreateObjectAsync(
        KubernetesClient client,
        V1Endpoints obj,
        CancellationToken cancellationToken)
    {
        return await client.CoreV1()
                           .CreateNamespacedEndpointsAsync(Namespace, obj, cancellationToken: cancellationToken)
                           .ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override async Task<V1Endpoints> ReplaceObjectAsync(
        KubernetesClient client,
        V1Endpoints obj,
        CancellationToken cancellationToken)
    {
        return await client.CoreV1()
                           .ReplaceNamespacedEndpointsAsync(Name, Namespace, obj, cancellationToken: cancellationToken)
                           .ConfigureAwait(false);
    }
}