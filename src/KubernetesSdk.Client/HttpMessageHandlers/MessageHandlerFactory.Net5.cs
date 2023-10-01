#if NET5_0_OR_GREATER

using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace Kubernetes.Client.HttpMessageHandlers;

public static partial class MessageHandlerFactory
{
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
            if (options.SslCaCerts != null)
            {
                var validator = new RemoteCertificateValidator(options.SslCaCerts);
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
