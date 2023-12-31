﻿// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Models;

namespace Kubernetes.Client.LeaderElection;

/// <summary>
/// Provides a annotation lock on a <see cref="V1ConfigMap"/>.
/// </summary>
public sealed class ConfigMapLock : KubernetesObjectAnnotationLock<V1ConfigMap>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigMapLock"/> class.
    /// </summary>
    /// <param name="namespace">The namespace of the object.</param>
    /// <param name="name">The name of the object.</param>
    /// <param name="identity">The identity of the lock owner.</param>
    public ConfigMapLock(string @namespace, string name, string identity)
        : base(@namespace, name, identity)
    {
    }

    /// <inheritdoc />
    protected override async Task<V1ConfigMap> ReadObjectAsync(
        KubernetesClient client,
        CancellationToken cancellationToken)
    {
        return await client.CoreV1()
                           .ReadNamespacedConfigMapAsync(Name, Namespace, cancellationToken: cancellationToken)
                           .ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override async Task<V1ConfigMap> CreateObjectAsync(
        KubernetesClient client,
        V1ConfigMap obj,
        CancellationToken cancellationToken)
    {
        return await client.CoreV1()
                           .CreateNamespacedConfigMapAsync(Namespace, obj, cancellationToken: cancellationToken)
                           .ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override async Task<V1ConfigMap> ReplaceObjectAsync(
        KubernetesClient client,
        V1ConfigMap obj,
        CancellationToken cancellationToken)
    {
        return await client.CoreV1()
                           .ReplaceNamespacedConfigMapAsync(Name, Namespace, obj, cancellationToken: cancellationToken)
                           .ConfigureAwait(false);
    }
}