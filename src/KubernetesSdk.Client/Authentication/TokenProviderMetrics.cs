// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using Kubernetes.Client.Diagnostics;

namespace Kubernetes.Client.Authentication;

internal sealed class TokenProviderMetrics
{
    private readonly string _tokenType;

    private readonly Counter<long> _totalRequests = KubernetesClientDefaults.Meter.CreateCounter<long>(
        "kubernetes.client.token.requests",
        "request",
        "The number of requests made to token provider.");

    private readonly Histogram<double> _requestDuration = KubernetesClientDefaults.Meter.CreateHistogram<double>(
        "kubernetes.client.token.duration",
        "ms",
        "The latency of requests made to the token provider.");

    public struct TrackedRequest : IDisposable
    {
        private readonly TokenProviderMetrics _metrics;
        private readonly TagList _requestTags;
        private readonly long _timestamp;
        private int _disposed;

        public TrackedRequest(TokenProviderMetrics metrics, TagList tags)
        {
            _metrics = metrics;
            _requestTags = tags;
            _timestamp = Stopwatch.GetTimestamp();
            _metrics._totalRequests.Add(1, tags);
        }

        public void Complete()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;

            long elapsedTicks = Stopwatch.GetTimestamp() - _timestamp;
            double elapsedMs = elapsedTicks / (double)TimeSpan.TicksPerMillisecond;
            _metrics._requestDuration.Record(elapsedMs, _requestTags);
        }
    }

    public TokenProviderMetrics(string tokenType)
    {
        _tokenType = tokenType;
    }

    public TrackedRequest TrackRequest()
    {
        TagList tags = default;
        tags.Add(OtelTags.TokenType, _tokenType);
        return new TrackedRequest(this, tags);
    }
}