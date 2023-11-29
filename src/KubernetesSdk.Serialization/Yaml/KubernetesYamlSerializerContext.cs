// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using YamlDotNet.Serialization;

namespace Kubernetes.Serialization.Yaml;

/// <summary>
/// Static YAML serializer context.
/// </summary>
[YamlStaticContext]
public partial class KubernetesYamlSerializerContext : StaticContext
{
}