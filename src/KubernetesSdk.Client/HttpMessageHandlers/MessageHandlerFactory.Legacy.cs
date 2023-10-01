#if !NET5_0_OR_GREATER

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
        Justification = "Ownership is transferred to HttpMessageHandlerWrapper.")]
    public static HttpMessageHandler CreatePrimaryHttpMessageHandler(KubernetesClientOptions options)
    {
        var result = new HttpClientHandler();
        if (options.SkipTlsVerify)
        {
            result.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        }
        else
        {
            if (options.SslCaCerts != null)
            {
                var validator = new RemoteCertificateValidator(options.SslCaCerts);
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
