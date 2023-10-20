// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Serialization.Json;

/// <summary>
/// Implements a serializer for Kubernetes objects using JSON.
/// </summary>
[SuppressMessage(
    "Trimming",
    "IL2026:Members annotated with \'RequiresUnreferencedCodeAttribute\' require dynamic access otherwise can break functionality when trimming application code",
    Justification = "The json serializer uses source generation.")]
public sealed class KubernetesJsonSerializer : IKubernetesSerializer
{
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesJsonSerializer"/> class.
    /// </summary>
    /// <param name="options">The <see cref="KubernetesJsonOptions"/>.</param>
    /// <exception cref="ArgumentNullException">Argument <paramref name="options"/> is <c>null</c>.</exception>
    public KubernetesJsonSerializer(KubernetesJsonOptions options)
    {
        Ensure.Arg.NotNull(options);
        _options = options.JsonSerializerOptions;
    }

    public async Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(stream);

        return await JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken)
                                   .ConfigureAwait(false);
    }

    public T? Deserialize<T>(Stream stream)
    {
        Ensure.Arg.NotNull(stream);
        return JsonSerializer.Deserialize<T>(stream, _options);
    }

    public T? Deserialize<T>(ReadOnlySpan<char> content)
    {
        return JsonSerializer.Deserialize<T>(content, _options);
    }

    public async Task SerializeAsync<T>(Stream stream, T value, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(stream);

        await JsonSerializer.SerializeAsync(stream, value, _options, cancellationToken)
                            .ConfigureAwait(false);
    }

    public void Serialize<T>(Stream stream, T value)
    {
        Ensure.Arg.NotNull(stream);
        JsonSerializer.Serialize(stream, value, _options);
    }

    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, _options);
    }
}