using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Models;

namespace Kubernetes.Client;

public interface IWatcher<T> : IDisposable, IAsyncDisposable
    where T : IKubernetesObject
{
    /// <summary>
    /// Reads the next event from the server.
    /// </summary>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="WatchEvent{T}"/>; <c>null</c> if the server closed the connection.</returns>
    Task<WatchEvent<T>?> ReadNextAsync(CancellationToken cancellationToken = default);

    // TODO: AsObservable()
}