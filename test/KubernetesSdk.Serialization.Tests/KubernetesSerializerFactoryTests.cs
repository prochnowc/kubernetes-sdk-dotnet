// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using NSubstitute;

namespace Kubernetes.Serialization;

public class KubernetesSerializerFactoryTests
{
    [Fact]
    public void CreateSerializerInvokesCorrespondingProvider()
    {
        var provider1 = Substitute.For<IKubernetesSerializerProvider>();
        provider1.ContentType.Returns("application/json");

        var provider2 = Substitute.For<IKubernetesSerializerProvider>();
        provider2.ContentType.Returns("application/yaml");

        var factory = new KubernetesSerializerFactory(new[] { provider1, provider2 });

        _ = factory.CreateSerializer("application/json");

        provider1.Received(1)
                 .CreateSerializer();

        _ = factory.CreateSerializer("application/yaml");

        provider2.Received(1)
                 .CreateSerializer();
    }
}