// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace Kubernetes.Client;

/// <summary>
/// The base class for all Kubernetes client exceptions.
/// </summary>
public class KubernetesClientException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesClientException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public KubernetesClientException(string? message)
        : this(message, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesClientException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public KubernetesClientException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}