namespace Kubernetes.Client.Operations;

public abstract class KubernetesClientOperations
{
    protected KubernetesClient Client { get; }

    protected KubernetesClientOperations(KubernetesClient client)
    {
        Client = client;
    }
}
