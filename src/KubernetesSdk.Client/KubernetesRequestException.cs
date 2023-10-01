using System;
using Kubernetes.Models;

namespace Kubernetes.Client;

public class KubernetesRequestException : KubernetesClientException
{
    public V1Status? Status { get; }

    public KubernetesRequestException(V1Status status)
        : this(status.Message, null)
    {
        Status = status;
    }

    public KubernetesRequestException(string? message)
        : this(message, null)
    {
    }

    public KubernetesRequestException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}