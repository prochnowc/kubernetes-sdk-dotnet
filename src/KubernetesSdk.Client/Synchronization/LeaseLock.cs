using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Models;

namespace Kubernetes.Client.Synchronization;

public class LeaseLock : MetaObjectLock<V1Lease>
{
    public LeaseLock(KubernetesClient client, string @namespace, string name, string identity)
        : base(client, @namespace, name, identity)
    {
    }

    /// <inheritdoc />
    protected override LeaderElectionRecord GetLeaderElectionRecord(V1Lease obj)
    {
        obj.Spec ??= new V1LeaseSpec();

        return new LeaderElectionRecord()
        {
            AcquireTime = obj.Spec.AcquireTime,
            HolderIdentity = obj.Spec.HolderIdentity,
            LeaderTransitions = obj.Spec.LeaseTransitions ?? 0,
            LeaseDurationSeconds = obj.Spec.LeaseDurationSeconds ?? 15, // 15 = default value
            RenewTime = obj.Spec.RenewTime,
        };
    }

    /// <inheritdoc />
    protected override void SetLeaderElectionRecord(V1Lease obj, LeaderElectionRecord record)
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
    protected override async Task<V1Lease> ReadMetaObjectAsync(CancellationToken cancellationToken)
    {
        return await Client.CoordinationV1()
                           .ReadNamespacedLeaseAsync(Name, Namespace, cancellationToken: cancellationToken)
                           .ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override async Task<V1Lease> CreateMetaObjectAsync(V1Lease obj, CancellationToken cancellationToken)
    {
        return await Client.CoordinationV1()
                           .CreateNamespacedLeaseAsync(Namespace, obj, cancellationToken: cancellationToken)
                           .ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override async Task<V1Lease> ReplaceMetaObjectAsync(V1Lease obj, CancellationToken cancellationToken)
    {
        return await Client.CoordinationV1()
                           .ReplaceNamespacedLeaseAsync(Name, Namespace, obj, cancellationToken: cancellationToken)
                           .ConfigureAwait(false);
    }
}