// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

#if NET7_0_OR_GREATER

using System.Net.Http;
using Kubernetes.Client.Http;

namespace Kubernetes.Client;

/// <content>
/// .NET 7.0 implementation of <see cref="KubernetesWebSocket"/>.
/// </content>
public sealed partial class KubernetesWebSocket
{
    internal KubernetesWebSocket(
        HttpClient client,
        string? protocol,
        KubernetesClientOptions options)
    {
        Ensure.Arg.NotNull(client);
        Ensure.Arg.NotNull(options);

        IClientWebSocket webSocket = WebSocketFactory();

        if (!string.IsNullOrEmpty(protocol))
            webSocket.Options.AddSubProtocol(protocol);

        _httpClient = client;
        _webSocket = webSocket;
        _options = options;
        _disposeCallback = null;
    }
}

#endif