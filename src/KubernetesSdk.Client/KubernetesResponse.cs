// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Models;
using Kubernetes.Serialization;

namespace Kubernetes.Client;

/// <summary>
/// Represents a Kubernetes API response.
/// </summary>
public sealed partial class KubernetesResponse : IDisposable
{
    private const string ContentType = "application/json";

    private readonly IKubernetesSerializerFactory _serializerFactory;
    private HttpResponseMessage? _response;
    private int _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesResponse"/> class.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
    /// <param name="serializerFactory">The <see cref="IKubernetesSerializerFactory"/>.</param>
    public KubernetesResponse(HttpResponseMessage response, IKubernetesSerializerFactory serializerFactory)
    {
        Ensure.Arg.NotNull(response);
        Ensure.Arg.NotNull(serializerFactory);

        _response = response;
        _serializerFactory = serializerFactory;
    }

    /// <summary>
    /// Ensures that the response has a success status code. Throws a <see cref="KubernetesRequestException"/> if the
    /// status code did not indicate success.
    /// </summary>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/>.</param>
    /// <exception cref="KubernetesRequestException">
    ///     The <see cref="KubernetesRequestException"/> which is thrown when the status code did not indicate success.
    /// </exception>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task EnsureSuccessStatusCodeAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        if (_response.IsSuccessStatusCode == false)
        {
            V1Status status;

            try
            {
                status = await ReadAsContentAsync<V1Status>(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception error)
            {
                throw new KubernetesRequestException(
                    $"The server returned an error but the response could not be deserialized: {_response.ReasonPhrase}",
                    error);
            }

            throw new KubernetesRequestException(status);
        }
    }

    /// <summary>
    /// Reads the response content as a <typeparamref name="T"/>.
    /// </summary>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/>.</param>
    /// <typeparam name="T">The type of the <see cref="IKubernetesObject"/>.</typeparam>
    /// <returns>The response content.</returns>
    /// <exception cref="KubernetesRequestException">The response content could not be deserialized.</exception>
    public async Task<T> ReadAsContentAsync<T>(CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        T? result;
        try
        {
            IKubernetesSerializer serializer =
                _serializerFactory.CreateSerializer(_response.Content.Headers.ContentType?.MediaType ?? ContentType);

            using Stream contentStream =
                await _response.Content.ReadAsStreamAsync(cancellationToken)
                               .ConfigureAwait(false);

            result = await serializer.DeserializeAsync<T>(contentStream, cancellationToken)
                                     .ConfigureAwait(false);
        }
        catch (Exception error)
        {
            throw new KubernetesRequestException("Failed to deserialize response", error);
        }

        if (result == null)
        {
            throw new KubernetesRequestException(
                "Failed to deserialize response, the server returned an empty response");
        }

        return result;
    }

    /// <summary>
    /// Reads the response as a stream of contents.
    /// </summary>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/>.</param>
    /// <typeparam name="T">The type of the <see cref="IKubernetesObject"/>.</typeparam>
    /// <returns>The <see cref="IWatcher{T}"/>.</returns>
    public async Task<IWatcher<T>> ReadAsWatcherAsync<T>(CancellationToken cancellationToken = default)
        where T : IKubernetesObject
    {
        EnsureNotDisposed();

        IKubernetesSerializer serializer =
            _serializerFactory.CreateSerializer(_response.Content.Headers.ContentType?.MediaType ?? ContentType);

        Watcher<T> watcher =
            await Watcher<T>.CreateAsync(_response, serializer, cancellationToken)
                            .ConfigureAwait(false);

        // response is disposed by the Watcher, we clear it here so that it is not disposed when this object is disposed
        _response = null;
        return watcher;
    }

    /// <summary>
    /// Reads the response as a byte stream.
    /// </summary>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="Stream"/>.</returns>
    public async Task<Stream> ReadAsStreamAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        WrappedStream stream =
            await WrappedStream.CreateAsync(_response, cancellationToken)
                               .ConfigureAwait(false);

        // response is disposed by the Stream, we clear it here so that it is not disposed when this object is disposed
        _response = null;
        return stream;
    }

    [MemberNotNull(nameof(_response))]
    private void EnsureNotDisposed()
    {
        if (_response == null)
            throw new ObjectDisposedException(nameof(KubernetesResponse));
    }

    /// <inheritdoc />
    [SuppressMessage(
        "IDisposableAnalyzers.Correctness",
        "IDISP007:Don\'t dispose injected",
        Justification = "Ownership is transferred from KubernetesClient.")]
    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
            return;

        _response?.Dispose();
        _response = null;
    }
}
