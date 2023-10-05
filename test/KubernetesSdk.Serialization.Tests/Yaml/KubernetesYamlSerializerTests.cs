// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using Kubernetes.Models;

namespace Kubernetes.Serialization.Yaml;

/// <summary>
/// Provides tests for the <see cref="KubernetesYamlSerializer"/>.
/// </summary>
public class KubernetesYamlSerializerTests : KubernetesSerializerTests<V1Deployment>
{
    protected override string Content => YamlContent.Deployment;

    protected override string NullContent => string.Empty;

    protected override IKubernetesSerializer CreateSerializer() => new KubernetesYamlSerializer(new KubernetesYamlOptions());
}