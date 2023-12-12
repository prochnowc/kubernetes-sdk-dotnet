// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Client.Http;

internal sealed class DefaultClientWebSocket : IClientWebSocket
{
    private readonly ClientWebSocket _webSocket = new ();

    public void Abort()
    {
        _webSocket.Abort();
    }

    public Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
    {
        return _webSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);
    }

    public Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
    {
        return _webSocket.CloseOutputAsync(closeStatus, statusDescription, cancellationToken);
    }

    public Task ConnectAsync(Uri uri, HttpMessageInvoker? invoker, CancellationToken cancellationToken)
    {
#if !NET7_0_OR_GREATER
        if (invoker != null)
            throw new NotSupportedException();

        return _webSocket.ConnectAsync(uri, cancellationToken);
#else
        return _webSocket.ConnectAsync(uri, invoker, cancellationToken);
#endif
    }

    public void Dispose()
    {
        _webSocket.Dispose();
    }

    public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        return _webSocket.ReceiveAsync(buffer, cancellationToken);
    }

    public Task SendAsync(
        ArraySegment<byte> buffer,
        WebSocketMessageType messageType,
        bool endOfMessage,
        CancellationToken cancellationToken)
    {
        return _webSocket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);
    }

    public WebSocketCloseStatus? CloseStatus => _webSocket.CloseStatus;

    public string? CloseStatusDescription => _webSocket.CloseStatusDescription;

    public ClientWebSocketOptions Options => _webSocket.Options;

    public WebSocketState State => _webSocket.State;

    public string? SubProtocol => _webSocket.SubProtocol;
}