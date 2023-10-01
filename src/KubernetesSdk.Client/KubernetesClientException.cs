using System;

namespace Kubernetes.Client;

public class KubernetesClientException : Exception
{
    public KubernetesClientException(string? message)
        : this(message, null)
    {
    }

    public KubernetesClientException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}