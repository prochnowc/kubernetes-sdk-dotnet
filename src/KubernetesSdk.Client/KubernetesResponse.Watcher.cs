// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Models;
using Kubernetes.Serialization;

namespace Kubernetes.Client;

public partial class KubernetesResponse
{
    private sealed class Watcher<T> : IWatcher<T>
        where T : IKubernetesObject
    {
        private readonly HttpResponseMessage _response;
        private readonly Stream _stream;
        private readonly IKubernetesSerializer _serializer;
        private readonly StreamReader _reader;
        private int _disposed;

        private Watcher(HttpResponseMessage response, Stream stream, IKubernetesSerializer serializer)
        {
            _response = response;
            _stream = stream;
            _serializer = serializer;
            _reader = new StreamReader(stream, Encoding.UTF8, false, 1024, true);
        }

        public static async Task<Watcher<T>> CreateAsync(
            HttpResponseMessage response,
            IKubernetesSerializer serializer,
            CancellationToken cancellationToken)
        {
            Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken)
                                          .ConfigureAwait(false);

            return new Watcher<T>(response, stream, serializer);
        }

        public async Task<WatchEvent<T>?> ReadNextAsync(CancellationToken cancellationToken)
        {
            EnsureNotDisposed();

            string? line = await _reader.ReadLineAsync(cancellationToken)
                                        .ConfigureAwait(false);

            if (line == null)
                return null;

            WatchEvent<T>? result;

            try
            {
                result = _serializer.Deserialize<WatchEvent<T>>(line.AsSpan());
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

        private void EnsureNotDisposed()
        {
            if (_disposed != 0)
                throw new ObjectDisposedException(nameof(KubernetesResponse));
        }

        [SuppressMessage(
            "IDisposableAnalyzers.Correctness",
            "IDISP007:Don\'t dispose injected",
            Justification = "Ownership is transferred from KubernetesResponse.")]
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;

            _reader.Dispose();
            _stream.Dispose();
            _response.Dispose();
        }

        [SuppressMessage(
            "IDisposableAnalyzers.Correctness",
            "IDISP007:Don\'t dispose injected",
            Justification = "Ownership is transferred from KubernetesResponse.")]
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;

            _reader.Dispose();
            await _stream.DisposeAsync()
                         .ConfigureAwait(false);
            _response.Dispose();
        }
    }
}