#if NET5_0_OR_GREATER

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Kubernetes.Client;

/// <summary>
/// Provides utility methods for loading certificates.
/// </summary>
internal static partial class CertificateUtils
{
    /// <summary>
    /// Load PEM encoded certificate file.
    /// </summary>
    /// <param name="path">Path to PEM encoded certificate.</param>
    /// <returns>List of X509 Certificates.</returns>
    public static X509Certificate2Collection LoadPem(string path)
    {
        var certCollection = new X509Certificate2Collection();
        using FileStream stream = File.OpenRead(path);
        certCollection.ImportFromPem(new StreamReader(stream).ReadToEnd());
        return certCollection;
    }

    /// <summary>
    /// Generates pfx from client configuration
    /// </summary>
    /// <param name="options">Kubernetes Client Configuration</param>
    /// <returns>Generated Pfx Path</returns>
    public static X509Certificate2 GetClientCertificate(KubernetesClientOptions options)
    {
        string? keyData = null;
        string? certData = null;

        if (!string.IsNullOrWhiteSpace(options.ClientCertificateKeyData))
        {
            keyData = Encoding.UTF8.GetString(Convert.FromBase64String(options.ClientCertificateKeyData));
        }

        if (!string.IsNullOrWhiteSpace(options.ClientKeyFilePath))
        {
            keyData = File.ReadAllText(options.ClientKeyFilePath);
        }

        if (keyData == null)
        {
            throw new InvalidOperationException("Client certificate key has not been configured");
        }

        if (!string.IsNullOrWhiteSpace(options.ClientCertificateData))
        {
            certData = Encoding.UTF8.GetString(Convert.FromBase64String(options.ClientCertificateData));
        }

        if (!string.IsNullOrWhiteSpace(options.ClientCertificateFilePath))
        {
            certData = File.ReadAllText(options.ClientCertificateFilePath);
        }

        if (certData == null)
        {
            throw new InvalidOperationException("Client certificate has not been configured");
        }

        var cert = X509Certificate2.CreateFromPem(certData, keyData);

        // see https://github.com/kubernetes-client/csharp/issues/737
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // This null password is to change the constructor to fix this KB:
            // https://support.microsoft.com/en-us/topic/kb5025823-change-in-how-net-applications-import-x-509-certificates-bf81c936-af2b-446e-9f7a-016f4713b46b
            string? nullPassword = null;

            cert = options.ClientCertificateKeyStoreFlags.HasValue
                ? new X509Certificate2(
                    cert.Export(X509ContentType.Pkcs12),
                    nullPassword,
                    options.ClientCertificateKeyStoreFlags.Value)
                : new X509Certificate2(cert.Export(X509ContentType.Pkcs12), nullPassword);
        }

        return cert;
    }
}

#endif