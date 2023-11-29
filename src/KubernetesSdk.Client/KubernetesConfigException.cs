// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace Kubernetes.Client;

/// <summary>
/// Exception that is thrown when there is an error in the configuration.
/// </summary>
public class KubernetesConfigException : KubernetesClientException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesConfigException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public KubernetesConfigException(string? message)
        : this(message, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesConfigException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public KubernetesConfigException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}