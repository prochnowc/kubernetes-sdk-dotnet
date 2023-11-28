// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Models;

namespace Kubernetes.Client.LeaderElection;

/// <summary>
/// Offers a common interface for locking on arbitrary resources used in leader election.
/// </summary>
public interface ILock
{
    /// <summary>
    /// Gets the locks Identity.
    /// </summary>
    string Identity { get; }

    /// <summary>
    /// Gets the <see cref="LeaderElectionRecord"/> of the object.
    /// </summary>
    /// <param name="client">The <see cref="KubernetesClient"/> used to get the Kubernetes object.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns>The <see cref="LeaderElectionRecord"/>.</returns>
    Task<LeaderElectionRecord> GetAsync(KubernetesClient client, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to create a <see cref="LeaderElectionRecord"/>.
    /// </summary>
    /// <param name="client">The <see cref="KubernetesClient"/> used to create the Kubernetes object.</param>
    /// <param name="record">The <see cref="LeaderElectionRecord"/> to create.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns><c>true</c> if created; <c>false</c> otherwise.</returns>
    Task<bool> CreateAsync(
        KubernetesClient client,
        LeaderElectionRecord record,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to update an existing <see cref="LeaderElectionRecord"/>.
    /// </summary>
    /// <param name="client">The <see cref="KubernetesClient"/> used to update the Kubernetes object.</param>
    /// <param name="record">The <see cref="LeaderElectionRecord"/> to create.</param>
    /// <param name="cancellationToken">Optional token to cancel the operation.</param>
    /// <returns><c>true</c> if updated; <c>false</c> otherwise.</returns>
    Task<bool> UpdateAsync(
        KubernetesClient client,
        LeaderElectionRecord record,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Describe is used to convert details on current resource lock into a string.
    /// </summary>
    /// <returns>Resource lock description.</returns>
    string Describe();
}