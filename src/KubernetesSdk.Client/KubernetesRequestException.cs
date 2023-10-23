using System;
using Kubernetes.Models;

namespace Kubernetes.Client;

public class KubernetesRequestException : KubernetesClientException
{
    public V1Status? Status { get; }

    public KubernetesRequestException(V1Status status)
        : this(status.Message, null)
    {
        Ensure.Arg.NotNull(status);

        // TODO: Add ToString() to V1Status
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