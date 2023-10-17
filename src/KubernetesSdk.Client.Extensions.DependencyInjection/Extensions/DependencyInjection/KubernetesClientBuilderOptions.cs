namespace Kubernetes.Client.Extensions.DependencyInjection;

internal sealed class KubernetesClientBuilderOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to use the <see cref="DefaultOptionsProvider"/>
    /// to configure options. Will be set to <c>false</c> if any explicit configuration is provided.
    /// </summary>
    public bool UseDefaultConfig { get; set; } = true;
}