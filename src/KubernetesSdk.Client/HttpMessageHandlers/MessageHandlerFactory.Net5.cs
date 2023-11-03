// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

#if NET5_0_OR_GREATER

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace Kubernetes.Client.HttpMessageHandlers;

public static partial class MessageHandlerFactory
{
    [SuppressMessage(
        "IDisposableAnalyzers.Correctness",
        "IDISP001:Dispose created",
        Justification = "SocketsHttpHandler is disposed by the HttpMessageHandlerWrapper.")]
    [SuppressMessage(
        "IDisposableAnalyzers.Correctness",
        "IDISP011:Don\'t return disposed instance",
        Justification = "Certificate is disposed by the HttpMessageHandlerWrapper.")]
    public static HttpMessageHandler CreatePrimaryHttpMessageHandler(KubernetesClientOptions options)
    {
        var result = new SocketsHttpHandler
        {
            KeepAlivePingPolicy = HttpKeepAlivePingPolicy.WithActiveRequests,
            KeepAlivePingDelay = TimeSpan.FromMinutes(3),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
            PooledConnectionLifetime = TimeSpan.FromMinutes(10),
            EnableMultipleHttp2Connections = true,
        };

        if (options.SkipTlsVerify)
        {
            result.SslOptions.RemoteCertificateValidationCallback = (_, _, _, _) => true;
        }
        else
        {
            if (options.CaCerts != null)
            {
                var validator = new RemoteCertificateValidator(options.CaCerts);
                result.SslOptions.RemoteCertificateValidationCallback = validator.CertificateValidationCallback;
            }
        }

        Action? disposeCallback = null;
        X509Certificate2? clientCertificate = CertificateUtils.TryGetClientCertificate(options);
        if (clientCertificate != null)
        {
            result.SslOptions.ClientCertificates ??= new X509Certificate2Collection();
            result.SslOptions.ClientCertificates.Add(clientCertificate);
            disposeCallback = () => clientCertificate.Dispose();

            // TODO this is workaround for net7.0, remove it when the issue is fixed
            // seems the client certificate is cached and cannot be updated
            result.SslOptions.LocalCertificateSelectionCallback = (_, _, _, _, _) => clientCertificate;
        }

        return new HttpMessageHandlerWrapper(result, disposeCallback);
    }
}
#endif
