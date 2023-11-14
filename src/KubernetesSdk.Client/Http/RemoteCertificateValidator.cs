// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Kubernetes.Client.Http;

internal sealed class RemoteCertificateValidator
{
    private readonly X509Certificate2Collection _caCerts;

    public RemoteCertificateValidator(X509Certificate2Collection caCerts)
    {
        _caCerts = caCerts;
    }

    public bool CertificateValidationCallback(
        object sender,
        X509Certificate? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        if (chain == null)
        {
            throw new ArgumentNullException(nameof(chain));
        }

        // If the certificate is a valid, signed certificate, return true.
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true;
        }

        // If there are errors in the certificate chain, look at each error to determine the cause.
        if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0)
        {
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            // Added our trusted certificates to the chain
            chain.ChainPolicy.ExtraStore.AddRange(_caCerts);

            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
            bool isValid = certificate != null && chain.Build((X509Certificate2)certificate);
            bool isTrusted = false;

            // Make sure that one of our trusted certs exists in the chain provided by the server.
            if (isValid)
            {
                foreach (X509Certificate2 cert in _caCerts)
                {
                    if (chain.Build(cert))
                    {
                        isTrusted = true;
                        break;
                    }
                }
            }

            return isValid && isTrusted;
        }

        // In all other cases, return false.
        return false;
    }
}