﻿// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Models;

namespace Kubernetes.Client.LeaderElection;

/// <summary>
/// Provides a lock using a <see cref="V1Lease"/>.
/// </summary>
public sealed class LeaseLock : KubernetesObjectLock<V1Lease>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LeaseLock"/> class.
    /// </summary>
    /// <param name="namespace">The namespace of the object.</param>
    /// <param name="name">The name of the object.</param>
    /// <param name="identity">The identity of the lock owner.</param>
    public LeaseLock(string @namespace, string name, string identity)
        : base(@namespace, name, identity)
    {
    }

    /// <inheritdoc />
    protected override LeaderElectionRecord GetLeaderElectionRecord(KubernetesClient client, V1Lease obj)
    {
        return new LeaderElectionRecord
        {
            AcquireTime = obj.Spec.AcquireTime,
            HolderIdentity = obj.Spec.HolderIdentity,
            LeaderTransitions = obj.Spec.LeaseTransitions ?? 0,
            LeaseDurationSeconds = obj.Spec.LeaseDurationSeconds ?? 15, // 15 = default value
            RenewTime = obj.Spec.RenewTime,
        };
    }

    /// <inheritdoc />
    protected override void SetLeaderElectionRecord(KubernetesClient client, V1Lease obj, LeaderElectionRecord record)
    {
        obj.Spec = new V1LeaseSpec()
        {
            AcquireTime = record.AcquireTime,
            HolderIdentity = record.HolderIdentity,
            LeaseTransitions = record.LeaderTransitions,
            LeaseDurationSeconds = record.LeaseDurationSeconds,
            RenewTime = record.RenewTime,
        };
    }

    /// <inheritdoc />
    protected override async Task<V1Lease> ReadObjectAsync(KubernetesClient client, CancellationToken cancellationToken)
    {
        return await client.CoordinationV1()
                           .ReadNamespacedLeaseAsync(Name, Namespace, cancellationToken: cancellationToken)
                           .ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override async Task<V1Lease> CreateObjectAsync(
        KubernetesClient client,
        V1Lease obj,
        CancellationToken cancellationToken)
    {
        return await client.CoordinationV1()
                           .CreateNamespacedLeaseAsync(Namespace, obj, cancellationToken: cancellationToken)
                           .ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override async Task<V1Lease> ReplaceObjectAsync(
        KubernetesClient client,
        V1Lease obj,
        CancellationToken cancellationToken)
    {
        return await client.CoordinationV1()
                           .ReplaceNamespacedLeaseAsync(Name, Namespace, obj, cancellationToken: cancellationToken)
                           .ConfigureAwait(false);
    }
}