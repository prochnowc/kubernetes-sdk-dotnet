// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

namespace Kubernetes.Serialization.Json;

/// <summary>
/// Provides a serializer for Kubernetes objects using JSON.
/// </summary>
public sealed class KubernetesJsonSerializerProvider : IKubernetesSerializerProvider
{
    private readonly KubernetesJsonOptions _options;

    /// <inheritdoc />
    public string ContentType => "application/json";

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesJsonSerializerProvider"/> class.
    /// </summary>
    /// <param name="options">The <see cref="KubernetesJsonOptions"/>.</param>
    public KubernetesJsonSerializerProvider(KubernetesJsonOptions? options = null)
    {
        _options = options ?? KubernetesJsonOptions.Default;
    }

    /// <inheritdoc />
    public IKubernetesSerializer CreateSerializer()
    {
        return new KubernetesJsonSerializer(_options);
    }
}
