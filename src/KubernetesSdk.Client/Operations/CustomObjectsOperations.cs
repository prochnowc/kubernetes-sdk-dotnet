using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Models;
using Kubernetes.Serialization;

namespace Kubernetes.Client.Operations;

public sealed partial class CustomObjectsOperations : KubernetesClientOperations
{
    public async Task<T> ListClusterCustomObjectAsync<T>(
        string? @continue = default,
        string? fieldSelector = default,
        string? labelSelector = default,
        int? limit = default,
        string? resourceVersion = default,
        string? resourceVersionMatch = default,
        bool? pretty = default,
        CancellationToken cancellationToken = default)
        where T : class, IKubernetesObject
    {
        KubernetesEntityType entityType = KubernetesEntityType.FromType<T>();
        return (T)await ListClusterCustomObjectAsync(
                entityType.Group,
                entityType.Version,
                entityType.Plural,
                @continue,
                fieldSelector,
                labelSelector,
                limit,
                resourceVersion,
                resourceVersionMatch,
                pretty,
                cancellationToken)
            .ConfigureAwait(false);
    }

    // TODO: watch
    public async Task<T> CreateClusterCustomObjectAsync<T>(
        T body,
        string? dryRun = default,
        string? fieldManager = default,
        bool? pretty = default,
        CancellationToken cancellationToken = default)
        where T : class, IKubernetesObject
    {
        KubernetesEntityType entityType = KubernetesEntityType.FromType<T>();
        return (T)await CreateClusterCustomObjectAsync(
                entityType.Group,
                entityType.Version,
                entityType.Plural,
                body,
                dryRun,
                fieldManager,
                pretty,
                cancellationToken)
            .ConfigureAwait(false);
    }
}