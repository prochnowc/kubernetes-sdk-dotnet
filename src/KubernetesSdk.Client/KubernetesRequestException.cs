// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using Kubernetes.Models;

namespace Kubernetes.Client;

/// <summary>
/// Exception that is thrown when a request to the Kubernetes API fails.
/// </summary>
public class KubernetesRequestException : KubernetesClientException
{
    /// <summary>
    /// Gets the status returned by the Kubernetes API.
    /// </summary>
    public V1Status? Status { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesRequestException"/> class.
    /// </summary>
    /// <param name="status">The <see cref="V1Status"/> returned by the Kubernetes API.</param>
    public KubernetesRequestException(V1Status status)
        : this(status.Message, null)
    {
        Ensure.Arg.NotNull(status);

        // TODO: Add ToString() to V1Status
        Status = status;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesRequestException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public KubernetesRequestException(string? message)
        : this(message, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesRequestException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public KubernetesRequestException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}