// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Serialization;
using NSubstitute;

namespace Kubernetes.Client.Mock;

public static class KubernetesTestClient
{
    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly IHttpMessageHandler _handler;

        public TestHttpMessageHandler(IHttpMessageHandler handler)
        {
            _handler = handler;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await _handler.SendAsync(request, cancellationToken);
        }
    }

    [SuppressMessage(
        "IDisposableAnalyzers.Correctness",
        "IDISP001:Dispose created",
        Justification = "Owned by KubernetesClient")]
    public static KubernetesClient Create(IHttpMessageHandler? handler = null)
    {
        handler ??= Substitute.For<IHttpMessageHandler>();

        var options = new KubernetesClientOptions();
        var httpClient = new HttpClient(new TestHttpMessageHandler(handler))
        {
            BaseAddress = new Uri("http://localhost"),
        };

        return new KubernetesClient(options, KubernetesSerializerFactory.Instance, httpClient, true);
    }
}