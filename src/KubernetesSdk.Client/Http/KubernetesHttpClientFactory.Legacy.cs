// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

#if !NET5_0_OR_GREATER

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace Kubernetes.Client.Http;

public static partial class KubernetesHttpClientFactory
{
    /// <summary>
    /// Creates the primary message handler used by the <see cref="HttpClient"/> of a <see cref="KubernetesClient"/>.
    /// </summary>
    /// <param name="options">The <see cref="KubernetesClientOptions"/>.</param>
    /// <returns>The primary <see cref="HttpMessageHandler"/>.</returns>
    [SuppressMessage(
        "IDisposableAnalyzers.Correctness",
        "IDISP001:Dispose created",
        Justification = "Ownership is transferred to HttpMessageHandlerWrapper.")]
    public static HttpMessageHandler CreatePrimaryMessageHandler(KubernetesClientOptions options)
    {
        var result = new HttpClientHandler();
        if (options.SkipTlsVerify)
        {
            result.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }
        else
        {
            if (options.CaCerts != null)
            {
                var validator = new RemoteCertificateValidator(options.CaCerts);
                result.ServerCertificateCustomValidationCallback = validator.CertificateValidationCallback;
            }
        }

        Action? disposeCallback = null;
        X509Certificate2? clientCertificate = CertificateUtils.TryGetClientCertificate(options);
        if (clientCertificate != null)
        {
            result.ClientCertificates.Add(clientCertificate);
            disposeCallback = () => clientCertificate.Dispose();
        }

        return new HttpMessageHandlerWrapper(result, disposeCallback);
    }
}

#endif
