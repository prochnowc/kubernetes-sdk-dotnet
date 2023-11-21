// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Net.Http;
using Kubernetes.Serialization;
using Microsoft.Extensions.Options;

namespace Kubernetes.Client;

internal sealed class KubernetesClientFactory : IKubernetesClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IKubernetesSerializerFactory _serializerFactory;
    private readonly IOptionsMonitor<KubernetesClientOptions> _clientOptionsSnapshot;

    internal static string GetHttpClientName(string name)
    {
        return name == string.Empty
            ? "KubernetesClient"
            : $"KubernetesClient-{name}";
    }

    public KubernetesClientFactory(
        IHttpClientFactory httpClientFactory,
        IKubernetesSerializerFactory serializerFactory,
        IOptionsMonitor<KubernetesClientOptions> clientOptionsSnapshot)
    {
        _httpClientFactory = httpClientFactory;
        _serializerFactory = serializerFactory;
        _clientOptionsSnapshot = clientOptionsSnapshot;
    }

    public KubernetesClient CreateClient(string name)
    {
        return new KubernetesClient(
            _clientOptionsSnapshot.Get(name),
            _serializerFactory,
            _ => _httpClientFactory.CreateClient(GetHttpClientName(name)));
    }
}