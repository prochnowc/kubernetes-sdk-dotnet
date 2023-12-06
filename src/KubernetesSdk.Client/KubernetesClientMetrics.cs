// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Http;
using System.Threading;
using Kubernetes.Client.Diagnostics;

namespace Kubernetes.Client;

internal sealed class KubernetesClientMetrics
{
    private readonly HttpClient _httpClient;

    private readonly Counter<long> _totalRequests = KubernetesClientDefaults.Meter.CreateCounter<long>(
        "kubernetes.client.requests",
        "request",
        "The number of requests made to the Kubernetes API server.");

    private readonly UpDownCounter<int> _activeRequests = KubernetesClientDefaults.Meter.CreateUpDownCounter<int>(
        "kubernetes.client.active_requests",
        "request",
        "The number of requests in flight to the Kubernetes API server.");

    private readonly Histogram<double> _requestDuration = KubernetesClientDefaults.Meter.CreateHistogram<double>(
        "kubernetes.client.duration",
        "ms",
        "The latency of requests made to the Kubernetes API server.");

    private readonly Counter<long> _statusCodes = KubernetesClientDefaults.Meter.CreateCounter<long>(
        "kubernetes.client.status_codes",
        description: "The number of status codes received from the Kubernetes API server.");

    public struct TrackedRequest : IDisposable
    {
        private readonly KubernetesClientMetrics _metrics;
        private readonly TagList _requestTags;
        private readonly long _timestamp;
        private int _disposed;

        public TrackedRequest(KubernetesClientMetrics metrics, KubernetesRequest request)
        {
            TagList tags = metrics.GetRequestTags(request);
            _metrics = metrics;
            _requestTags = tags;
            _timestamp = Stopwatch.GetTimestamp();
            _metrics._totalRequests.Add(1, tags);
            _metrics._activeRequests.Add(1, tags);
        }

        public void Complete(KubernetesResponse response)
        {
            TagList tags = _metrics.GetResponseTags(response);
            _metrics._statusCodes.Add(1, tags);
            Dispose();
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;

            long elapsedTicks = Stopwatch.GetTimestamp() - _timestamp;
            double elapsedMs = elapsedTicks / (double)TimeSpan.TicksPerMillisecond;
            _metrics._requestDuration.Record(elapsedMs, _requestTags);
            _metrics._activeRequests.Add(-1, _requestTags);
        }
    }

    public KubernetesClientMetrics(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private void AddPeerTags(TagList tags)
    {
        Uri? peer = _httpClient.BaseAddress;
        tags.Add(OtelTags.NetPeerName, peer?.Host);
        tags.Add(OtelTags.NetPeerPort, peer?.IsDefaultPort == true ? null : peer?.Port);
    }

    private TagList GetRequestTags(KubernetesRequest request)
    {
        TagList tags = request.GetRequestTags();
        AddPeerTags(tags);
        return tags;
    }

    private TagList GetResponseTags(KubernetesResponse request)
    {
        TagList tags = request.GetResponseTags();
        AddPeerTags(tags);
        return tags;
    }

    public TrackedRequest TrackRequest(KubernetesRequest request)
    {
        return new TrackedRequest(this, request);
    }
}