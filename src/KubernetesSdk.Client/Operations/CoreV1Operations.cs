// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Client.Operations;

/// <content>
/// POD exec operations.
/// </content>
public sealed partial class CoreV1Operations : KubernetesClientOperations
{
    public async Task<KubernetesWebSocket> ConnectNamespacedPodExecAsync(
        string name,
        string @namespace,
        IEnumerable<string> command,
        string? container = null,
        bool stdin = true,
        bool stdout = true,
        bool stderr = true,
        bool tty = true,
        CancellationToken cancellationToken = default)
    {
        string requestUriTemplate = "api/v1/namespaces/{namespace}/pods/{name}/exec";

        RequestUriBuilder requestUriBuilder =
            new RequestUriBuilder(requestUriTemplate)
                .AddPathParameter("name", name)
                .AddPathParameter("namespace", @namespace)
                .AddQueryParameter("command", string.Join(" ", command))
                .AddQueryParameter("container", container)
                .AddQueryParameter("stdin", stdin ? "1" : "0")
                .AddQueryParameter("stdout", stdout ? "1" : "0")
                .AddQueryParameter("stderr", stderr ? "1" : "0")
                .AddQueryParameter("tty", tty ? "1" : "0");

        return await Client.ConnectAsync(
                               requestUriBuilder.ToUri(),
                               KubernetesWebSocketProtocol.V4BinaryWebsocketProtocol,
                               cancellationToken)
                           .ConfigureAwait(false);
    }
}