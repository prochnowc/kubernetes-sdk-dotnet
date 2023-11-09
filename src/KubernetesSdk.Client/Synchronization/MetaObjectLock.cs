using System;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Models;

namespace Kubernetes.Client.Synchronization;

public abstract class MetaObjectLock<T> : ILock
    where T : class, IKubernetesObject, IMetadata<V1ObjectMeta?>, new()
{
    private T? _object;

    /// <inheritdoc />
    public string Identity { get; }

    protected KubernetesClient Client { get; }

    protected string Namespace { get; }

    protected string Name { get; }

    protected MetaObjectLock(KubernetesClient client, string @namespace, string name, string identity)
    {
        Client = client;
        Namespace = @namespace;
        Name = name;
        Identity = identity;
    }

    protected abstract LeaderElectionRecord GetLeaderElectionRecord(T obj);

    protected abstract void SetLeaderElectionRecord(T obj, LeaderElectionRecord record);

    /// <inheritdoc />
    public async Task<LeaderElectionRecord> GetAsync(CancellationToken cancellationToken = default)
    {
        T obj = await ReadMetaObjectAsync(cancellationToken)
            .ConfigureAwait(false);

        LeaderElectionRecord record = GetLeaderElectionRecord(obj);
        Interlocked.Exchange(ref _object, obj);

        return record;
    }

    protected abstract Task<T> ReadMetaObjectAsync(CancellationToken cancellationToken);

    /// <inheritdoc />
    public async Task<bool> CreateAsync(LeaderElectionRecord record, CancellationToken cancellationToken = default)
    {
        var obj = new T
        {
            Metadata = new V1ObjectMeta
            {
                Name = Name,
                Namespace = Namespace,
            },
        };

        SetLeaderElectionRecord(obj, record);

        try
        {
            T createdObj = await CreateMetaObjectAsync(obj, cancellationToken)
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

    protected abstract Task<T> CreateMetaObjectAsync(T obj, CancellationToken cancellationToken);

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(LeaderElectionRecord record, CancellationToken cancellationToken = default)
    {
        T? obj = Interlocked.CompareExchange(ref _object, null, null);
        if (obj == null)
        {
            throw new InvalidOperationException("endpoint not initialized, call get or create first");
        }

        SetLeaderElectionRecord(obj, record);

        try
        {
            obj = await ReplaceMetaObjectAsync(obj, cancellationToken)
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

    protected abstract Task<T> ReplaceMetaObjectAsync(T obj, CancellationToken cancellationToken);

    /// <inheritdoc />
    public string Describe()
    {
        return $"{Namespace}/{Name}";
    }
}