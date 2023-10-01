// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

namespace Kubernetes.Serialization.Yaml;

/// <summary>
/// Provides a serializer for Kubernetes objects using YAML.
/// </summary>
public sealed class KubernetesYamlSerializerProvider : IKubernetesSerializerProvider
{
    private readonly KubernetesYamlOptions _options;

    /// <inheritdoc />
    public string ContentType => "application/json";

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesYamlSerializerProvider"/> class.
    /// </summary>
    /// <param name="options">The <see cref="KubernetesYamlOptions"/>.</param>
    public KubernetesYamlSerializerProvider(KubernetesYamlOptions? options = null)
    {
        _options = options ?? KubernetesYamlOptions.Default;
    }

    /// <inheritdoc />
    public IKubernetesSerializer CreateSerializer()
    {
        return new KubernetesYamlSerializer(_options);
    }
}