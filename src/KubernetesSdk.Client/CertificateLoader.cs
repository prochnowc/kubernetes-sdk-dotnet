// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography.X509Certificates;

namespace Kubernetes.Client;

/// <summary>
/// Provides utility methods for loading certificates.
/// </summary>
internal static partial class CertificateLoader
{
    /// <summary>
    /// Tries to load the X509 Certificate authorities from the client options.
    /// </summary>
    /// <param name="options">The Kubernetes client options.</param>
    /// <returns>X509 Client certificate collection.</returns>
    public static X509Certificate2Collection? TryLoadCertificateAuthorities(KubernetesClientOptions options)
    {
        X509Certificate2Collection? certificateAuthorities = null;
        if (!string.IsNullOrEmpty(options.CertificateAuthorityData))
        {
            certificateAuthorities = LoadCertificateBundle(options.CertificateAuthorityData);
        }
        else if (!string.IsNullOrEmpty(options.CertificateAuthorityFilePath))
        {
            certificateAuthorities = LoadCertificateBundleFile(options.CertificateAuthorityFilePath);
        }

        return certificateAuthorities;
    }

    /// <summary>
    /// Tries to load the X509 Client Certificate from the client options.
    /// </summary>
    /// <param name="options">The Kubernetes client options.</param>
    /// <returns>X509 Client certificate.</returns>
    public static X509Certificate2? TryLoadClientCertificate(KubernetesClientOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ClientCertificateData)
            && !string.IsNullOrWhiteSpace(options.ClientCertificateKeyData))
        {
            return LoadClientCertificate(options.ClientCertificateData, options.ClientCertificateKeyData);
        }

        if (!string.IsNullOrWhiteSpace(options.ClientCertificateFilePath)
            && !string.IsNullOrWhiteSpace(options.ClientCertificateKeyFilePath))
        {
            return LoadClientCertificateFile(options.ClientCertificateFilePath, options.ClientCertificateKeyFilePath);
        }

        return null;
    }
}