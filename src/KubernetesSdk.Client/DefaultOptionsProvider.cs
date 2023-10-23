using System.Collections.Generic;
using Kubernetes.Client.KubeConfig;
using Kubernetes.Serialization;

namespace Kubernetes.Client;

public class DefaultOptionsProvider : IKubernetesClientOptionsProvider
{
    private readonly IEnumerable<IAuthProviderOptionsBinder> _authProviderOptionBinders;
    private readonly IKubernetesSerializerFactory _serializerFactory;

    public DefaultOptionsProvider()
        : this(
            new IAuthProviderOptionsBinder[]
            {
                new AzureAuthProviderOptionsBinder(),
                new GcpAuthProviderOptionsBinder(),
                new OidcAuthProviderOptionsBinder(),
            },
            KubernetesSerializerFactory.Instance)
    {
    }

    public DefaultOptionsProvider(
        IEnumerable<IAuthProviderOptionsBinder> authProviderOptionBinders,
        IKubernetesSerializerFactory serializerFactory)
    {
        Ensure.Arg.NotNull(authProviderOptionBinders);
        Ensure.Arg.NotNull(serializerFactory);

        _authProviderOptionBinders = authProviderOptionBinders;
        _serializerFactory = serializerFactory;
    }

    public KubernetesClientOptions CreateOptions()
    {
        var options = new KubernetesClientOptions();
        BindOptions(options);
        return options;
    }

    public virtual void BindOptions(KubernetesClientOptions options)
    {
        Ensure.Arg.NotNull(options);

        if (InClusterOptionsProvider.IsInCluster())
        {
            var clusterOptionsProvider = new InClusterOptionsProvider();
            clusterOptionsProvider.BindOptions(options);
            return;
        }

        var kubeConfigOptionsProvider = new KubeConfigOptionsProvider(_authProviderOptionBinders, _serializerFactory);
        kubeConfigOptionsProvider.BindOptions(options);
    }
}