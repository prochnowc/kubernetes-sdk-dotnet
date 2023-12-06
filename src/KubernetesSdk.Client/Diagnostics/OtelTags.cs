// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

namespace Kubernetes.Client.Diagnostics;

internal static class OtelTags
{
    public const string CodeNamespace = "code.namespace";
    public const string CodeFunction = "code.function";
    public const string CodeFilePath = "code.filepath";
    public const string CodeLineNo = "code.lineno";

    public const string NetPeerName = "net.peer.name";
    public const string NetPeerPort = "net.peer.port";

    public const string KubernetesStatusCode = "kubernetes.status_code";
    public const string KubernetesApiVersion = "kubernetes.api_version";
    public const string KubernetesKind = "kubernetes.kind";
    public const string KubernetesAction = "kubernetes.action";

    public const string TokenType = "kubernetes.client.token.type";
    public const string TokenExpiresAt = "kubernetes.client.token.expires_at";
}