// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Kubernetes.Models;

namespace Kubernetes.Serialization;

/// <summary>
/// Provides tests for an implementation of the <see cref="IKubernetesSerializer"/>.
/// </summary>
/// <typeparam name="T">The type of the serialized object.</typeparam>
public abstract class KubernetesSerializerTests<T>
{
    /// <summary>
    /// Gets the serialized content of <typeparamref name="T"/>.
    /// </summary>
    protected abstract string Content { get; }

    /// <summary>
    /// Gets the <c>null</c> content of <typeparamref name="T"/>.
    /// </summary>
    protected abstract string NullContent { get; }

    [Fact]
    public async Task CanDeserializeStreamAsync()
    {
        using Stream stream = await CreateStreamAsync(Content);

        IKubernetesSerializer serializer = CreateSerializer();
        var obj = await serializer.DeserializeAsync<T>(stream);

        obj.Should()
           .NotBeNull();
    }

    [Fact]
    public async Task CanDeserializeNullFromStreamAsync()
    {
        using Stream stream = await CreateStreamAsync(NullContent);

        IKubernetesSerializer serializer = CreateSerializer();
        var obj = await serializer.DeserializeAsync<T>(stream);

        obj.Should()
           .BeNull();
    }

    [Fact]
    public void CanDeserializeStream()
    {
        using Stream stream = CreateStream(Content);

        IKubernetesSerializer serializer = CreateSerializer();
        var obj = serializer.Deserialize<T>(stream);

        obj.Should()
           .NotBeNull();
    }

    [Fact]
    public void CanDeserializeNullFromStream()
    {
        using Stream stream = CreateStream(NullContent);

        IKubernetesSerializer serializer = CreateSerializer();
        var obj = serializer.Deserialize<T>(stream);

        obj.Should()
           .BeNull();
    }

    [Fact]
    public void CanDeserializerSpan()
    {
        IKubernetesSerializer serializer = CreateSerializer();
        var obj = serializer.Deserialize<T>(Content.AsSpan());

        obj.Should()
           .NotBeNull();
    }

    [Fact]
    public void CanDeserializerNullFromSpan()
    {
        IKubernetesSerializer serializer = CreateSerializer();
        var obj = serializer.Deserialize<T>(NullContent.AsSpan());

        obj.Should()
           .BeNull();
    }

    /// <summary>
    /// Creates the serializer implementation.
    /// </summary>
    /// <returns>The <see cref="IKubernetesSerializer"/>.</returns>
    protected abstract IKubernetesSerializer CreateSerializer();

    private Stream CreateStream(string content)
    {
        var stream = new MemoryStream(content.Length);
        using var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    private async Task<Stream> CreateStreamAsync(string content)
    {
        var stream = new MemoryStream(content.Length);
        using var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true);
        await writer.WriteAsync(content);
        await writer.FlushAsync();
        stream.Position = 0;
        return stream;
    }
}