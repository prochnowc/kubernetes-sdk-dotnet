// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AppCore.Diagnostics;
using Kubernetes.Models;

namespace Kubernetes.Serialization.Json;

/// <summary>
/// Implements a serializer for Kubernetes objects using JSON.
/// </summary>
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

    private IKubernetesObject? Deserialize(JsonElement element)
    {
        string? apiVersion = element.GetProperty("apiVersion")
                                    .GetString();

        string? kind = element.GetProperty("kind")
                              .GetString();

        if (string.IsNullOrWhiteSpace(apiVersion)
            || string.IsNullOrWhiteSpace(kind))
        {
            throw new InvalidOperationException(
                "Cannot deserialize Kubernetes object because apiVersion and/or kind is empty.");
        }

        string group = string.Empty;
        string version;

        int versionDelimiter = apiVersion!.IndexOf('/');
        if (versionDelimiter != -1)
        {
            group = apiVersion.Substring(0, versionDelimiter);
            version = apiVersion.Substring(versionDelimiter + 1);
        }
        else
        {
            version = apiVersion;
        }

        return (IKubernetesObject?)element.Deserialize(
            KubernetesEntityType.FromGroupVersionKind(group, version, kind!).Type,
            _options);
    }

    public async Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(stream);

        if (typeof(T) == typeof(IKubernetesObject))
        {
            var element = (JsonElement?)await JsonSerializer
                                              .DeserializeAsync<object>(stream, _options, cancellationToken)
                                              .ConfigureAwait(false);

            return element != null
                ? (T?)Deserialize((JsonElement)element)
                : default;
        }

        return await JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken)
                                   .ConfigureAwait(false);
    }

    public T? Deserialize<T>(Stream stream)
    {
        Ensure.Arg.NotNull(stream);

        if (typeof(T) == typeof(IKubernetesObject))
        {
            var element = (JsonElement?)JsonSerializer.Deserialize<object>(stream, _options);
            return element != null
                ? (T?)Deserialize((JsonElement)element)
                : default;
        }

        return JsonSerializer.Deserialize<T>(stream, _options);
    }

    public T? Deserialize<T>(ReadOnlySpan<char> content)
    {
        if (typeof(T) == typeof(IKubernetesObject))
        {
            var element = (JsonElement?)JsonSerializer.Deserialize<object>(content, _options);
            return element != null
                ? (T?)Deserialize((JsonElement)element)
                : default;
        }

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