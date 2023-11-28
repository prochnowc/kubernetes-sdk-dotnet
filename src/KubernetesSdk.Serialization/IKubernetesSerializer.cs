// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Serialization;

/// <summary>
/// Represents a serializer and deserializer for Kubernetes objects.
/// </summary>
public interface IKubernetesSerializer
{
    /// <summary>
    /// Deserializes an object from a stream.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to deserialize.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <returns>The deserialized object.</returns>
    Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deserializes an object from a stream.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to deserialize.</param>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <returns>The deserialized object.</returns>
    T? Deserialize<T>(Stream stream);

    /// <summary>
    /// Deserializes an object from a char span.
    /// </summary>
    /// <param name="content">The char span to deserialize.</param>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <returns>The deserialized object.</returns>
    T? Deserialize<T>(ReadOnlySpan<char> content);

    /// <summary>
    /// Serializes an object to a stream.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to serialize to.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <returns>Asynchronous task.</returns>
    Task SerializeAsync<T>(Stream stream, T value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Serializes an object to a stream.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to serialize to.</param>
    /// <param name="value">The object to serialize.</param>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    void Serialize<T>(Stream stream, T value);

    /// <summary>
    /// Serializes an object to a string.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <returns>The serialized object.</returns>
    string Serialize<T>(T value);
}