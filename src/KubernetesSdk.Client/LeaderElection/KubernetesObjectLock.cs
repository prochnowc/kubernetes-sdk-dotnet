// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Models;

namespace Kubernetes.Client.LeaderElection;

/// <summary>
/// Provides a base class for a lock on a Kubernetes object.
/// </summary>
/// <typeparam name="T">The type of the <see cref="IKubernetesObject{TMetadata}"/>.</typeparam>
public abstract class KubernetesObjectLock<T> : ILock
    where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private T? _object;

    /// <inheritdoc />
    public string Identity { get; }

    /// <summary>
    /// Gets the namespace of the object.
    /// </summary>
    protected string Namespace { get; }

    /// <summary>
    /// Gets the name of the object.
    /// </summary>
    protected string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesObjectLock{T}"/> class.
    /// </summary>
    /// <param name="namespace">The namespace of the object.</param>
    /// <param name="name">The name of the object.</param>
    /// <param name="identity">The identity of the lock owner.</param>
    protected KubernetesObjectLock(string @namespace, string name, string identity)
    {
        Ensure.Arg.NotEmpty(@namespace);
        Ensure.Arg.NotEmpty(name);
        Ensure.Arg.NotEmpty(identity);

        Namespace = @namespace;
        Name = name;
        Identity = identity;
    }

    /// <summary>
    /// Gets the <see cref="LeaderElectionRecord"/> of the object.
    /// </summary>
    /// <param name="client">The <see cref="KubernetesClient"/> used to communicate with the Kubernetes API.</param>
    /// <param name="obj">The object to get the <see cref="LeaderElectionRecord"/> from.</param>
    /// <returns>The <see cref="LeaderElectionRecord"/>.</returns>
    protected abstract LeaderElectionRecord GetLeaderElectionRecord(KubernetesClient client, T obj);

    /// <summary>
    /// Sets the <see cref="LeaderElectionRecord"/> of the object.
    /// </summary>
    /// <param name="client">The <see cref="KubernetesClient"/> used to communicate with the Kubernetes API.</param>
    /// <param name="obj">The object to set the <see cref="LeaderElectionRecord"/> on.</param>
    /// <param name="record">The <see cref="LeaderElectionRecord"/> to set on the object.</param>
    protected abstract void SetLeaderElectionRecord(KubernetesClient client, T obj, LeaderElectionRecord record);

    /// <inheritdoc />
    public async Task<LeaderElectionRecord> GetAsync(
        KubernetesClient client,
        CancellationToken cancellationToken = default)
    {
        T obj = await ReadObjectAsync(client, cancellationToken)
            .ConfigureAwait(false);

        LeaderElectionRecord record = GetLeaderElectionRecord(client, obj);
        Interlocked.Exchange(ref _object, obj);

        return record;
    }

    /// <summary>
    /// Reads the Kubernetes object from the  server.
    /// </summary>
    /// <param name="client">The <see cref="KubernetesClient"/> used to get the Kubernetes object.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>The Kubernetes object.</returns>
    protected abstract Task<T> ReadObjectAsync(KubernetesClient client, CancellationToken cancellationToken);

    /// <inheritdoc />
    public async Task<bool> CreateAsync(
        KubernetesClient client,
        LeaderElectionRecord record,
        CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(record);

        var obj = new T
        {
            Metadata = new V1ObjectMeta
            {
                Name = Name,
                Namespace = Namespace,
            },
        };

        SetLeaderElectionRecord(client, obj, record);

        try
        {
            T createdObj = await CreateObjectAsync(client, obj, cancellationToken)
                .ConfigureAwait(false);

            Interlocked.Exchange(ref _object, createdObj);
            return true;
        }
        catch (KubernetesRequestException)
        {
            // ignore
        }

        return false;
    }

    /// <summary>
    /// Creates the Kubernetes object on the server.
    /// </summary>
    /// <param name="client">The <see cref="KubernetesClient"/> used to get create the Kubernetes object.</param>
    /// <param name="obj">The object to create.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>The created Kubernetes object.</returns>
    protected abstract Task<T> CreateObjectAsync(KubernetesClient client, T obj, CancellationToken cancellationToken);

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(
        KubernetesClient client,
        LeaderElectionRecord record,
        CancellationToken cancellationToken = default)
    {
        T? obj = Interlocked.CompareExchange(ref _object, null, null);
        if (obj == null)
        {
            throw new InvalidOperationException("endpoint not initialized, call get or create first");
        }

        SetLeaderElectionRecord(client, obj, record);

        try
        {
            obj = await ReplaceObjectAsync(client, obj, cancellationToken)
                .ConfigureAwait(false);

            Interlocked.Exchange(ref _object, obj);
            return true;
        }
        catch (KubernetesRequestException)
        {
            // ignore
        }

        return false;
    }

    /// <summary>
    /// Replaces the Kubernetes object on the server.
    /// </summary>
    /// <param name="client">The <see cref="KubernetesClient"/> used to replace the Kubernetes object.</param>
    /// <param name="obj">The object to replace.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>The replaced Kubernetes object.</returns>
    protected abstract Task<T> ReplaceObjectAsync(KubernetesClient client, T obj, CancellationToken cancellationToken);

    /// <inheritdoc />
    public string Describe()
    {
        return $"{Namespace}/{Name}";
    }
}