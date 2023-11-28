// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Kubernetes.Serialization.Yaml;

/// <summary>
/// Implements a serializer for Kubernetes objects using JSON.
/// </summary>
public sealed class KubernetesYamlSerializer : IKubernetesSerializer
{
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesYamlSerializer"/> class.
    /// </summary>
    /// <param name="options">The <see cref="KubernetesYamlOptions"/>.</param>
    /// <exception cref="ArgumentNullException">Argument <paramref name="options"/> is <c>null</c>.</exception>
    public KubernetesYamlSerializer(KubernetesYamlOptions options)
    {
        Ensure.Arg.NotNull(options);
        _serializer = options.BuildSerializer();
        _deserializer = options.BuildDeserializer();
    }

    /// <inheritdoc />
    public async Task<T?> DeserializeAsync<T>(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, false, 1024, true);
        string content = await reader.ReadToEndAsync(cancellationToken)
                                     .ConfigureAwait(false);

        return Deserialize<T>(content.AsSpan());
    }

    /// <inheritdoc />
    public T? Deserialize<T>(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, false, 1024, true);
        string content = reader.ReadToEnd();
        return Deserialize<T>(content.AsSpan());
    }

    /// <inheritdoc />
    public T? Deserialize<T>(ReadOnlySpan<char> content)
    {
        return _deserializer.Deserialize<T?>(content.ToString());
    }

    /// <inheritdoc />
    public async Task SerializeAsync<T>(
        Stream stream,
        T value,
        CancellationToken cancellationToken = default)
    {
        using var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true);
        string content = Serialize(value);
        await writer.WriteAsync(content.AsMemory(), cancellationToken)
                    .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Serialize<T>(Stream stream, T value)
    {
        using var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true);
        string content = Serialize(value);
        writer.Write(content);
    }

    /// <inheritdoc />
    public string Serialize<T>(T value)
    {
        return value == null
            ? _serializer.Serialize(value)
            : _serializer.Serialize(value, value.GetType());
    }
}