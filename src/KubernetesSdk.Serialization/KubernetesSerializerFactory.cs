using System;
using System.Collections.Generic;
using System.Linq;
using Kubernetes.Serialization.Json;
using Kubernetes.Serialization.Yaml;

namespace Kubernetes.Serialization;

/// <summary>
/// Provides the default implementation of the <see cref="IKubernetesSerializerFactory"/> interface.
/// </summary>
public sealed class KubernetesSerializerFactory : IKubernetesSerializerFactory
{
    private static readonly Lazy<KubernetesSerializerFactory> LazyInstance = new (
        () => new KubernetesSerializerFactory(
            new IKubernetesSerializerProvider[]
            {
                new KubernetesJsonSerializerProvider(),
                new KubernetesYamlSerializerProvider(),
            }));

    private readonly IEnumerable<IKubernetesSerializerProvider> _providers;

    /// <summary>
    /// Gets the default instance of the <see cref="KubernetesSerializerFactory"/> class.
    /// </summary>
    public static KubernetesSerializerFactory Instance => LazyInstance.Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesSerializerFactory"/> class.
    /// </summary>
    /// <param name="providers">An enumerable of <see cref="IKubernetesSerializerProvider"/> instances.</param>
    public KubernetesSerializerFactory(IEnumerable<IKubernetesSerializerProvider> providers)
    {
        Ensure.Arg.NotNull(providers);
        _providers = providers;
    }

    /// <inheritdoc />
    public IKubernetesSerializer CreateSerializer(string contentType)
    {
        Ensure.Arg.NotNull(contentType);

        IKubernetesSerializerProvider? provider = _providers.FirstOrDefault(
            p => string.Equals(contentType, p.ContentType, StringComparison.OrdinalIgnoreCase));

        if (provider == null)
        {
            throw new ArgumentException($"No serializer registered for content type '{contentType}'");
        }

        return provider.CreateSerializer();
    }
}