// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using Kubernetes.Models;

namespace Kubernetes.Serialization.Json;

/// <summary>
/// Provides tests for the <see cref="KubernetesJsonSerializer"/>.
/// </summary>
public class KubernetesJsonSerializerTests : KubernetesSerializerTests<V1Deployment>
{
    protected override string Content => JsonContent.Deployment;

    protected override string NullContent => "null";

    protected override IKubernetesSerializer CreateSerializer() => new KubernetesJsonSerializer(new KubernetesJsonOptions());
}