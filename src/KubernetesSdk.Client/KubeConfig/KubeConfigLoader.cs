// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Models.KubeConfig;
using Kubernetes.Serialization;

namespace Kubernetes.Client.KubeConfig;

/// <summary>
/// Provides a loader for Kubernetes client configuration files.
/// </summary>
public sealed class KubeConfigLoader
{
    private static readonly string DefaultKubeConfigPath =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".kube",
            "config");

    private readonly IKubernetesSerializerFactory _serializerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="KubeConfigLoader"/> class.
    /// </summary>
    /// <remarks>
    /// Uses the default <see cref="KubernetesSerializerFactory"/> instance.
    /// </remarks>
    public KubeConfigLoader()
        : this(KubernetesSerializerFactory.Instance)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KubeConfigLoader"/> class.
    /// </summary>
    /// <param name="serializerFactory">The <see cref="IKubernetesSerializerFactory"/>.</param>
    public KubeConfigLoader(IKubernetesSerializerFactory serializerFactory)
    {
        Ensure.Arg.NotNull(serializerFactory);
        _serializerFactory = serializerFactory;
    }

    /// <summary>
    /// Gets the path to the Kubernetes client configuration file.
    /// </summary>
    /// <remarks>
    /// Resolves the path from the environment variable <c>KUBECONFIG</c> if set; otherwise uses the default path.
    /// </remarks>
    /// <returns>The path to the Kubernetes client configuration file.</returns>
    public static string GetKubeConfigPath()
    {
        string? kubeConfigPath = Environment.GetEnvironmentVariable("KUBECONFIG");
        return !string.IsNullOrWhiteSpace(kubeConfigPath)
            ? kubeConfigPath
            : DefaultKubeConfigPath;
    }

    /// <summary>
    /// Loads the Kubernetes client configuration file using the specified path.
    /// </summary>
    /// <param name="path">The path to the Kubernetes client configuration file; If <c>null</c> the result from <see cref="GetKubeConfigPath"/> is used.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>The loaded <see cref="V1Config"/>.</returns>
    public async Task<V1Config> LoadAsync(string? path = null, CancellationToken cancellationToken = default)
    {
        path ??= GetKubeConfigPath();
        using FileStream stream = File.OpenRead(path);
        return await LoadAsync(stream, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Loads the Kubernetes client configuration file using the specified path.
    /// </summary>
    /// <param name="path">
    /// The path to the Kubernetes client configuration file;
    /// If <c>null</c> the result from <see cref="GetKubeConfigPath"/> is used.
    /// </param>
    /// <returns>The loaded <see cref="V1Config"/>.</returns>
    public V1Config Load(string? path = null)
    {
        path ??= GetKubeConfigPath();
        using FileStream stream = File.OpenRead(path);
        return Load(stream);
    }

    /// <summary>
    /// Loads the Kubernetes client configuration file from the specified stream.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> used to read the configuration.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>The loaded <see cref="V1Config"/>.</returns>
    public async Task<V1Config> LoadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        IKubernetesSerializer serializer = _serializerFactory.CreateSerializer("application/yaml");
        return await serializer.DeserializeAsync<V1Config>(stream, cancellationToken)
                               .ConfigureAwait(false) ?? new V1Config();
    }

    /// <summary>
    /// Loads the Kubernetes client configuration file from the specified stream.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> used to read the configuration.</param>
    /// <returns>The loaded <see cref="V1Config"/>.</returns>
    public V1Config Load(Stream stream)
    {
        IKubernetesSerializer serializer = _serializerFactory.CreateSerializer("application/yaml");
        return serializer.Deserialize<V1Config>(stream) ?? new V1Config();
    }
}