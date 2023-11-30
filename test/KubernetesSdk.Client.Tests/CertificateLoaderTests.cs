// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;

namespace Kubernetes.Client;

public class CertificateLoaderTests
{
    [Fact]
    public void CanLoadClientCertificateFile()
    {
        using X509Certificate2 certificate = CertificateLoader.LoadClientCertificateFile(
            "assets/client-cert.crt",
            "assets/client-cert.key");

        using RSA? privateKey = certificate.GetRSAPrivateKey();
        privateKey.Should()
                  .NotBeNull();
    }

    [Fact]
    public void CanLoadClientEllipticCertificateFile()
    {
        using X509Certificate2 certificate = CertificateLoader.LoadClientCertificateFile(
            "assets/client-cert-elliptic.crt",
            "assets/client-cert-elliptic.key");

        using ECDsa? privateKey = certificate.GetECDsaPrivateKey();
        privateKey.Should()
                  .NotBeNull();
    }

    [Fact]
    public void CanLoadClientCertificate()
    {
        using var certReader = new StreamReader(File.OpenRead("assets/client-cert-data.txt"));
        using var keyReader = new StreamReader(File.OpenRead("assets/client-cert-key-data.txt"));

        using X509Certificate2 certificate = CertificateLoader.LoadClientCertificate(
            certReader.ReadToEnd(),
            keyReader.ReadToEnd());

        using RSA? privateKey = certificate.GetRSAPrivateKey();
        privateKey.Should()
                  .NotBeNull();
    }

    [Fact]
    public void CanLoadCertificateBundleFile()
    {
        X509Certificate2Collection certificates = CertificateLoader.LoadCertificateBundleFile("assets/ca-bundle.crt");

        using var intermediateCert = new X509Certificate2("assets/ca-bundle-intermediate.crt");
        using var rootCert = new X509Certificate2("assets/ca-bundle-root.crt");

        certificates.Count.Should()
                    .Be(2);

        certificates[0]
            .RawData.Should()
            .Equal(intermediateCert.RawData);

        certificates[1]
            .RawData.Should()
            .Equal(rootCert.RawData);
    }

    [Fact]
    public void CanLoadCertificateBundle()
    {
        using var reader = new StreamReader(File.OpenRead("assets/ca-bundle-data.txt"));
        X509Certificate2Collection certificates = CertificateLoader.LoadCertificateBundle(reader.ReadToEnd());

        using var intermediateCert = new X509Certificate2("assets/ca-bundle-intermediate.crt");
        using var rootCert = new X509Certificate2("assets/ca-bundle-root.crt");

        certificates.Count.Should()
                    .Be(2);

        certificates[0]
            .RawData.Should()
            .Equal(intermediateCert.RawData);

        certificates[1]
            .RawData.Should()
            .Equal(rootCert.RawData);
    }
}