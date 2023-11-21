// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

namespace Kubernetes.Client.Extensions.DependencyInjection;

internal sealed class KubernetesClientBuilderOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to use the <see cref="DefaultOptionsProvider"/>
    /// to configure options. Will be set to <c>false</c> if any explicit configuration is provided.
    /// </summary>
    public bool UseDefaultConfig { get; set; } = true;
}