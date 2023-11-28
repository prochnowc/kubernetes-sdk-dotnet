// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
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

    public static KubernetesClient Create(IHttpMessageHandler? handler = null)
    {
        handler ??= Substitute.For<IHttpMessageHandler>();

        var options = new KubernetesClientOptions();
        var httpClient = new HttpClient(new TestHttpMessageHandler(handler))
        {
            BaseAddress = new Uri("http://localhost"),
        };

        return new KubernetesClient(options, KubernetesSerializerFactory.Instance, _ => httpClient);
    }
}