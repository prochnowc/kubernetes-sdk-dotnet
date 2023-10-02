using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Kubernetes.Client.KubeConfig;
using Kubernetes.KubeConfig.Models;
using Kubernetes.Serialization;
using NSubstitute;

namespace Kubernetes.Client;

public class KubeConfigOptionsProviderTests
{
    [SuppressMessage(
        "Non-substitutable member",
        "NS1000:Non-virtual setup specification.",
        Justification = "Protected method is virtual.")]
    [SuppressMessage(
        "Non-substitutable member",
        "NS1004:Argument matcher used with a non-virtual member of a class.",
        Justification = "Protected method is virtual.")]
    private KubeConfigOptionsProvider CreateProvider(
        IEnumerable<IAuthProviderOptionsBinder> authProviderOptionBinder,
        Func<V1Config> config)
    {
        var serializer = Substitute.For<IKubernetesSerializerFactory>();
        var provider = Substitute.ForPartsOf<KubeConfigOptionsProvider>(authProviderOptionBinder, serializer);
        provider.Protected("LoadConfig", Arg.Any<string>())
                .Returns(ci => config());

        return provider;
    }

    private KubeConfigOptionsProvider CreateProvider(Func<V1Config> config)
    {
        return CreateProvider(Enumerable.Empty<IAuthProviderOptionsBinder>(), config);
    }

    [Fact]
    public void OptionsArePopulatedFromConfig()
    {
        string @namespace = "default";
        string host = "https://localhost";

        V1Config config = new ()
        {
            CurrentContext = "test",
            Contexts =
            {
                new Context()
                {
                    Name = "test",
                    ContextDetails = new ContextDetails()
                    {
                        Cluster = "localhost",
                        Namespace = @namespace,
                    },
                },
            },
            Clusters =
            {
                new Cluster()
                {
                    Name = "localhost",
                    ClusterEndpoint = new ClusterEndpoint()
                    {
                        Server = host,
                    },
                },
            },
        };

        KubeConfigOptionsProvider provider = CreateProvider(() => config);
        KubernetesClientOptions options = provider.CreateOptions();

        options.Namespace.Should()
               .Be(@namespace);

        options.Host.Should()
               .Be(host);
    }
}