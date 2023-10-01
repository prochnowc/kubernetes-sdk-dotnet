using System;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Models;

namespace Kubernetes.Client;

public interface IWatcher<T> : IDisposable, IAsyncDisposable
    where T : IKubernetesObject
{
    Task<WatchEvent<T>?> ReadNextAsync(CancellationToken cancellationToken);

    // TODO: AsObservable()
}