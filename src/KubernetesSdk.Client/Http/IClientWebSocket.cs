// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Client.Http;

internal interface IClientWebSocket : IDisposable
{
    void Abort();

    Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken);

    Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken);

    Task ConnectAsync(Uri uri, HttpMessageInvoker? invoker, CancellationToken cancellationToken);

    Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken);

    Task SendAsync(
        ArraySegment<byte> buffer,
        WebSocketMessageType messageType,
        bool endOfMessage,
        CancellationToken cancellationToken);

    WebSocketCloseStatus? CloseStatus { get; }

    string? CloseStatusDescription { get; }

    ClientWebSocketOptions Options { get; }

    WebSocketState State { get; }

    string? SubProtocol { get; }
}