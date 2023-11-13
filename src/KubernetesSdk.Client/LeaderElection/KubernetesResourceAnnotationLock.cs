// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using Kubernetes.Models;
using Kubernetes.Serialization;

namespace Kubernetes.Client.LeaderElection;

/// <summary>
/// Provides a base class for a lock on a Kubernetes object using annotations.
/// </summary>
/// <typeparam name="T">The type of the <see cref="IKubernetesObject{TMetadata}"/>.</typeparam>
public abstract class KubernetesResourceAnnotationLock<T> : KubernetesResourceLock<T>
    where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private const string AnnotationKey = "control-plane.alpha.kubernetes.io/leader";

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesResourceAnnotationLock{T}"/> class.
    /// </summary>
    /// <param name="client">The <see cref="KubernetesClient"/> used to communicate with the Kubernetes API server.</param>
    /// <param name="namespace">The namespace of the object.</param>
    /// <param name="name">The name of the object.</param>
    /// <param name="identity">The identity of the lock owner.</param>
    protected KubernetesResourceAnnotationLock(KubernetesClient client, string @namespace, string name, string identity)
        : base(client, @namespace, name, identity)
    {
    }

    /// <inheritdoc />
    protected override LeaderElectionRecord GetLeaderElectionRecord(T obj)
    {
        string? recordContent = obj.GetAnnotation(AnnotationKey);

        LeaderElectionRecord? record = null;
        if (!string.IsNullOrEmpty(recordContent))
        {
            IKubernetesSerializer serializer = Client.SerializerFactory.CreateSerializer("application/json");
            record = serializer.Deserialize<LeaderElectionRecord>(recordContent.AsSpan());
        }

        return record ?? new LeaderElectionRecord();
    }

    /// <inheritdoc />
    protected override void SetLeaderElectionRecord(T obj, LeaderElectionRecord record)
    {
        IKubernetesSerializer serializer = Client.SerializerFactory.CreateSerializer("application/json");
        obj.SetAnnotation(AnnotationKey, serializer.Serialize(record));
    }
}