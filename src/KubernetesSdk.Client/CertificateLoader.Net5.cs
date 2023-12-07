// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

#if NET5_0_OR_GREATER

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Kubernetes.Client;

/// <summary>
/// Provides utility methods for loading certificates.
/// </summary>
internal static partial class CertificateLoader
{
    /// <summary>
    /// Load a PEM encoded certificate bundle.
    /// </summary>
    /// <param name="path">Path to PEM encoded certificate bundle.</param>
    /// <returns>List of X509 Certificates.</returns>
    public static X509Certificate2Collection LoadCertificateBundleFile(string path)
    {
        var certCollection = new X509Certificate2Collection();
        certCollection.ImportFromPem(Encoding.UTF8.GetString(FileSystem.ReadAllBytes(path)));
        return certCollection;
    }

    /// <summary>
    /// Load a Base64 PEM encoded certificate bundle.
    /// </summary>
    /// <param name="data">Base64 PEM encoded certificate bundle.</param>
    /// <returns>List of X509 Certificates.</returns>
    public static X509Certificate2Collection LoadCertificateBundle(string data)
    {
        var certCollection = new X509Certificate2Collection();
        certCollection.ImportFromPem(Encoding.UTF8.GetString(Convert.FromBase64String(data)));
        return certCollection;
    }

    [SuppressMessage(
        "IDisposableAnalyzers.Correctness",
        "IDISP007:Don\'t dispose injected",
        Justification = "Passed in certificate is copied and returned to the caller.")]
    [SuppressMessage(
        "IDisposableAnalyzers.Correctness",
        "IDISP015:Member should not return created and cached instance",
        Justification = "Returned certificate must always be disposed.")]
    private static X509Certificate2 FixCertificate(X509Certificate2 cert)
    {
        // see https://github.com/kubernetes-client/csharp/issues/737
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // This null password is to change the constructor to fix this KB:
            // https://support.microsoft.com/en-us/topic/kb5025823-change-in-how-net-applications-import-x-509-certificates-bf81c936-af2b-446e-9f7a-016f4713b46b
            string? nullPassword = null;

            using (cert)
            {
                return new X509Certificate2(cert.Export(X509ContentType.Pkcs12), nullPassword);
            }
        }

        return cert;
    }

    /// <summary>
    /// Loads the X509 Client Certificate from files.
    /// </summary>
    /// <param name="certPath">Path to client certificate file.</param>
    /// <param name="keyPath">Path to client certificate key file.</param>
    /// <returns>X509 Client certificate.</returns>
    public static X509Certificate2 LoadClientCertificateFile(string certPath, string keyPath)
    {
        var cert = X509Certificate2.CreateFromPem(FileSystem.ReadAllText(certPath), FileSystem.ReadAllText(keyPath));
        return FixCertificate(cert);
    }

    /// <summary>
    /// Loads the X509 Client Certificate from Base64 encoded data.
    /// </summary>
    /// <param name="certData">Base64 encoded certificate.</param>
    /// <param name="keyData">Base64 encoded key.</param>
    /// <returns>X509 Client certificate.</returns>
    public static X509Certificate2 LoadClientCertificate(string certData, string keyData)
    {
        certData = Encoding.UTF8.GetString(Convert.FromBase64String(certData));
        keyData = Encoding.UTF8.GetString(Convert.FromBase64String(keyData));

        var cert = X509Certificate2.CreateFromPem(certData, keyData);
        return FixCertificate(cert);
    }
}

#endif