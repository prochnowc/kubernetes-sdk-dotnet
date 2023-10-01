using System.Security.Cryptography.X509Certificates;

namespace Kubernetes.Client;

/// <summary>
/// Provides utility methods for loading certificates.
/// </summary>
internal static partial class CertificateUtils
{
    /// <summary>
    /// Tries to load the X509 Client Certificate from the client options.
    /// </summary>
    /// <param name="options">The Kubernetes client options.</param>
    /// <returns>X509 Client certificate.</returns>
    public static X509Certificate2? TryGetClientCertificate(KubernetesClientOptions options)
    {
        if ((!string.IsNullOrWhiteSpace(options.ClientCertificateData) ||
             !string.IsNullOrWhiteSpace(options.ClientCertificateFilePath)) &&
            (!string.IsNullOrWhiteSpace(options.ClientCertificateKeyData) ||
             !string.IsNullOrWhiteSpace(options.ClientKeyFilePath)))
        {
            return GetClientCertificate(options);
        }

        return null;
    }
}