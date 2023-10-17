using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Models;
using Kubernetes.Serialization;

namespace Kubernetes.Client;

public sealed partial class KubernetesResponse : IDisposable
{
    private readonly IKubernetesSerializerFactory _serializerFactory;
    private HttpResponseMessage? _response;
    private int _disposed;

    public KubernetesResponse(HttpResponseMessage response, IKubernetesSerializerFactory serializerFactory)
    {
        _response = response;
        _serializerFactory = serializerFactory;
    }

    public async Task EnsureSuccessStatusCodeAsync(CancellationToken cancellationToken)
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

    public async Task<T> ReadAsContentAsync<T>(CancellationToken cancellationToken)
    {
        EnsureNotDisposed();

        T? result;
        try
        {
            IKubernetesSerializer serializer =
                _serializerFactory.CreateSerializer(_response!.Content.Headers.ContentType.MediaType);

            using Stream contentStream =
                await _response!.Content.ReadAsStreamAsync(cancellationToken)
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

    public async Task<IWatcher<T>> ReadAsWatcherAsync<T>(CancellationToken cancellationToken)
        where T : IKubernetesObject
    {
        EnsureNotDisposed();

        Watcher<T> watcher =
            await Watcher<T>.CreateAsync(_response, _serializerFactory, cancellationToken)
                            .ConfigureAwait(false);

        // response is disposed by the Stream, we clear it here so that it is not disposed when this object is disposed
        _response = null;
        return watcher;
    }

    public async Task<Stream> ReadAsStreamAsync(CancellationToken cancellationToken)
    {
        EnsureNotDisposed();

        WrappedStream stream =
            await WrappedStream.CreateAsync(_response, cancellationToken)
                               .ConfigureAwait(false);

        // response is disposed by the Stream, we clear it here so that it is not disposed when this object is disposed
        _response = null;
        return stream;
    }

    // TODO: [MemberNotNull(nameof(_response))]
    private void EnsureNotDisposed()
    {
        if (_response == null)
            throw new ObjectDisposedException(nameof(KubernetesResponse));
    }

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
