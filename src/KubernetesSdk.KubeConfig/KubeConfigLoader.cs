using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.KubeConfig.Models;
using Kubernetes.Serialization;

namespace Kubernetes.KubeConfig;

public class KubeConfigLoader
{
    private static readonly string DefaultKubeConfigPath =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".kube",
            "config");

    private readonly IKubernetesSerializerFactory _serializerFactory;

    public KubeConfigLoader()
        : this(KubernetesSerializerFactory.Instance)
    {
    }

    public KubeConfigLoader(IKubernetesSerializerFactory serializerFactory)
    {
        _serializerFactory = serializerFactory;
    }

    public static string GetKubeConfigPath()
    {
        return Environment.GetEnvironmentVariable("KUBECONFIG")
               ?? DefaultKubeConfigPath;
    }

    public async Task<V1Config> LoadAsync(string? path = null, CancellationToken cancellationToken = default)
    {
        path ??= GetKubeConfigPath();
        using FileStream stream = File.OpenRead(path);
        return await LoadAsync(stream, cancellationToken)
            .ConfigureAwait(false);
    }

    public V1Config Load(string? path = null)
    {
        path ??= GetKubeConfigPath();
        using FileStream stream = File.OpenRead(path);
        return Load(stream);
    }

    public async Task<V1Config> LoadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        IKubernetesSerializer serializer = _serializerFactory.CreateSerializer("application/yaml");
        return await serializer.DeserializeAsync<V1Config>(stream, cancellationToken)
                               .ConfigureAwait(false);
    }

    public V1Config Load(Stream stream)
    {
        IKubernetesSerializer serializer = _serializerFactory.CreateSerializer("application/yaml");
        return serializer.Deserialize<V1Config>(stream);
    }
}