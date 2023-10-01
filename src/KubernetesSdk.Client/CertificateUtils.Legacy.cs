#if !NET5_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

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

        IList<X509Certificate> certs = new X509CertificateParser().ReadCertificates(stream);

        // Convert BouncyCastle X509Certificates to the .NET cryptography implementation and add
        // it to the certificate collection
        foreach (X509Certificate cert in certs)
        {
            // This null password is to change the constructor to fix this KB:
            // https://support.microsoft.com/en-us/topic/kb5025823-change-in-how-net-applications-import-x-509-certificates-bf81c936-af2b-446e-9f7a-016f4713b46b
            string? nullPassword = null;
            certCollection.Add(new X509Certificate2(cert.GetEncoded(), nullPassword));
        }

        return certCollection;
    }

    /// <summary>
    /// Loads the X509 Client Certificate from the client options.
    /// </summary>
    /// <param name="options">The Kubernetes client options.</param>
    /// <returns>X509 Client certificate.</returns>
    public static X509Certificate2 GetClientCertificate(KubernetesClientOptions options)
    {
        byte[]? keyData = null;
        byte[]? certData = null;

        if (!string.IsNullOrWhiteSpace(options.ClientCertificateKeyData))
        {
            keyData = Convert.FromBase64String(options.ClientCertificateKeyData !);
        }

        if (!string.IsNullOrWhiteSpace(options.ClientKeyFilePath))
        {
            keyData = File.ReadAllBytes(options.ClientKeyFilePath !);
        }

        if (keyData == null)
        {
            throw new InvalidOperationException("Client certificate key has not been configured");
        }

        if (!string.IsNullOrWhiteSpace(options.ClientCertificateData))
        {
            certData = Convert.FromBase64String(options.ClientCertificateData !);
        }

        if (!string.IsNullOrWhiteSpace(options.ClientCertificateFilePath))
        {
            certData = File.ReadAllBytes(options.ClientCertificateFilePath !);
        }

        if (certData == null)
        {
            throw new InvalidOperationException("Client certificate has not been configured");
        }

        X509Certificate? cert = new X509CertificateParser().ReadCertificate(certData);

        // key usage is a bit string, zero-th bit is 'digitalSignature'
        // See https://www.alvestrand.no/objectid/2.5.29.15.html for more details.
        if (cert != null && cert.GetKeyUsage() != null && !cert.GetKeyUsage()[0])
        {
            throw new InvalidOperationException(
                "Client certificates must be marked for digital signing. " +
                "See https://github.com/kubernetes-client/csharp/issues/319");
        }

        object obj;
        using (var reader = new StreamReader(new MemoryStream(keyData)))
        using (var pemReader = new PemReader(reader))
        {
            obj = pemReader.ReadObject();
            if (obj is AsymmetricCipherKeyPair key)
            {
                AsymmetricCipherKeyPair cipherKey = key;
                obj = cipherKey.Private;
            }
        }

        var keyParams = (AsymmetricKeyParameter)obj;

        Pkcs12Store? store = new Pkcs12StoreBuilder().Build();
        store.SetKeyEntry("K8SKEY", new AsymmetricKeyEntry(keyParams), new[] { new X509CertificateEntry(cert) });

        using var pkcs = new MemoryStream();

        store.Save(pkcs, Array.Empty<char>(), new SecureRandom());

        // This null password is to change the constructor to fix this KB:
        // https://support.microsoft.com/en-us/topic/kb5025823-change-in-how-net-applications-import-x-509-certificates-bf81c936-af2b-446e-9f7a-016f4713b46b
        string? nullPassword = null;

        if (options.ClientCertificateKeyStoreFlags.HasValue)
        {
            return new X509Certificate2(pkcs.ToArray(), nullPassword, options.ClientCertificateKeyStoreFlags.Value);
        }
        else
        {
            return new X509Certificate2(pkcs.ToArray(), nullPassword);
        }
    }
}

#endif
