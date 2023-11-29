// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Kubernetes.Client.KubeConfig;
using Kubernetes.Serialization;

namespace Kubernetes.Client;

/// <summary>
/// Default options provider that populates <see cref="KubernetesClientOptions"/> from kubeconfig file
/// or in-cluster.
/// </summary>
public class DefaultOptionsProvider : IKubernetesClientOptionsProvider
{
    private readonly IEnumerable<IAuthProviderOptionsBinder> _authProviderOptionBinders;
    private readonly IKubernetesSerializerFactory _serializerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultOptionsProvider"/> class.
    /// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultOptionsProvider"/> class.
    /// </summary>
    /// <param name="authProviderOptionBinders">The authentication provider option binders.</param>
    /// <param name="serializerFactory">The <see cref="IKubernetesSerializerFactory"/>.</param>
    public DefaultOptionsProvider(
        IEnumerable<IAuthProviderOptionsBinder> authProviderOptionBinders,
        IKubernetesSerializerFactory serializerFactory)
    {
        Ensure.Arg.NotNull(authProviderOptionBinders);
        Ensure.Arg.NotNull(serializerFactory);

        _authProviderOptionBinders = authProviderOptionBinders;
        _serializerFactory = serializerFactory;
    }

    /// <inheritdoc />
    public KubernetesClientOptions CreateOptions()
    {
        var options = new KubernetesClientOptions();
        BindOptions(options);
        return options;
    }

    /// <inheritdoc />
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