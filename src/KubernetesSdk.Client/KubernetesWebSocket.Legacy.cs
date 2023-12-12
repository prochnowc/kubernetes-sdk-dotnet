// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

#if !NET7_0_OR_GREATER

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Kubernetes.Client.Http;

namespace Kubernetes.Client;

/// <content>
/// Legacy implementation of <see cref="KubernetesWebSocket"/> for .NET below 7.0.
/// </content>
public sealed partial class KubernetesWebSocket
{
    [SuppressMessage(
        "IDisposableAnalyzers.Correctness",
        "IDISP001:Dispose created",
        Justification = "Disposed by KubernetesdWebSocket via callback.")]
    internal KubernetesWebSocket(HttpClient client, string? protocol, KubernetesClientOptions options)
    {
        Ensure.Arg.NotNull(client);
        Ensure.Arg.NotNull(options);

        IClientWebSocket webSocket = WebSocketFactory();

        if (!string.IsNullOrEmpty(protocol))
            webSocket.Options.AddSubProtocol(protocol);

        webSocket.Options.SetRequestHeader(
            HttpHeaderNames.UserAgent,
            (options.UserAgent ?? KubernetesClientDefaults.UserAgent).ToString());

        if (!string.IsNullOrWhiteSpace(options.TlsServerName))
        {
            webSocket.Options.SetRequestHeader(HttpHeaderNames.Host, options.TlsServerName);
        }

        Action? disposeCallback = null;

        try
        {
            // setup client certificate
            X509Certificate2? certificate = CertificateLoader.TryLoadClientCertificate(options);
            if (certificate != null)
            {
                webSocket.Options.ClientCertificates.Add(certificate);
                disposeCallback += () => certificate.Dispose();
            }

            if (options.SkipTlsVerify)
            {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                webSocket.Options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
#else
                throw new NotSupportedException("The web socket implementation does not support skipping TLS verification.");
#endif
            }
            else
            {
                X509Certificate2Collection? certificateAuthorities =
                    CertificateLoader.TryLoadCertificateAuthorities(options);

                if (certificateAuthorities != null)
                {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                    var validator = new RemoteCertificateValidator(certificateAuthorities);
                    webSocket.Options.RemoteCertificateValidationCallback = validator.CertificateValidationCallback;

                    disposeCallback += () =>
                    {
                        foreach (X509Certificate2 certificateAuthority in certificateAuthorities)
                        {
                            certificateAuthority.Dispose();
                        }
                    };
#else
                    throw new NotSupportedException("The web socket implementation does not support custom CA certificates.");
#endif
                }
            }
        }
        catch
        {
            disposeCallback?.Invoke();
            throw;
        }

        _httpClient = client;
        _webSocket = webSocket;
        _options = options;
        _disposeCallback = disposeCallback;
    }
}

#endif
