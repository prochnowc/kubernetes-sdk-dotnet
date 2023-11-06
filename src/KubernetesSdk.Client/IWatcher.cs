// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Models;

namespace Kubernetes.Client;

/// <summary>
/// Represents the watcher for resource changes.
/// </summary>
/// <typeparam name="T">The type of the watched resource.</typeparam>
public interface IWatcher<T> : IDisposable, IAsyncDisposable
    where T : IKubernetesObject
{
    /// <summary>
    /// Reads the next event from the server.
    /// </summary>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="WatchEvent{T}"/>; <c>null</c> if the server closed the connection.</returns>
    /// <exception cref="KubernetesRequestException">There was an error processing the response.</exception>
    Task<WatchEvent<T>?> ReadNextAsync(CancellationToken cancellationToken = default);
}