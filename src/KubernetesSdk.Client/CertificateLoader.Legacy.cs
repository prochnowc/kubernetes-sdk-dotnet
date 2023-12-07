// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

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
internal static partial class CertificateLoader
{
    /// <summary>
    /// Load a PEM encoded certificate bundle file.
    /// </summary>
    /// <param name="path">Path to PEM encoded certificate bundle.</param>
    /// <returns>List of X509 Certificates.</returns>
    public static X509Certificate2Collection LoadCertificateBundleFile(string path)
    {
        return LoadCertificateBundle(FileSystem.ReadAllBytes(path));
    }

    /// <summary>
    /// Load a Base64 PEM encoded certificate.
    /// </summary>
    /// <param name="data">Base64 PEM encoded certificate.</param>
    /// <returns>List of X509 Certificates.</returns>
    public static X509Certificate2Collection LoadCertificateBundle(string data)
    {
        return LoadCertificateBundle(Convert.FromBase64String(data));
    }

    private static X509Certificate2Collection LoadCertificateBundle(byte[] data)
    {
        var certCollection = new X509Certificate2Collection();

        IList<X509Certificate> certs = new X509CertificateParser().ReadCertificates(data);

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
    /// Loads the X509 Client Certificate from files.
    /// </summary>
    /// <param name="certPath">Path to client certificate file.</param>
    /// <param name="keyPath">Path to client certificate key file.</param>
    /// <returns>X509 Client certificate.</returns>
    public static X509Certificate2 LoadClientCertificateFile(string certPath, string keyPath)
    {
        return LoadClientCertificate(FileSystem.ReadAllBytes(certPath), FileSystem.ReadAllBytes(keyPath));
    }

    /// <summary>
    /// Loads the X509 Client Certificate from Base64 encoded data.
    /// </summary>
    /// <param name="certData">Base64 encoded certificate.</param>
    /// <param name="keyData">Base64 encoded key.</param>
    /// <returns>X509 Client certificate.</returns>
    public static X509Certificate2 LoadClientCertificate(string certData, string keyData)
    {
        return LoadClientCertificate(Convert.FromBase64String(certData), Convert.FromBase64String(keyData));
    }

    private static X509Certificate2 LoadClientCertificate(byte[] certData, byte[] keyData)
    {
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

        return new X509Certificate2(pkcs.ToArray(), nullPassword);
    }
}

#endif
