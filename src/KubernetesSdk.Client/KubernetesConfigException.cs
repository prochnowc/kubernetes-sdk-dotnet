using System;

namespace Kubernetes.Client;

public class KubernetesConfigException : KubernetesClientException
{
    public KubernetesConfigException(string? message)
        : this(message, null)
    {
    }

    public KubernetesConfigException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}