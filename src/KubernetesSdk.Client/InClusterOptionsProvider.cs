// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Runtime.InteropServices;
using Kubernetes.Client.Authentication;

namespace Kubernetes.Client;

/// <summary>
/// Populates <see cref="KubernetesClientOptions"/> from environment variables and service account
/// when running inside cluster.
/// </summary>
public class InClusterOptionsProvider : IKubernetesClientOptionsProvider
{
    private const string ServiceAccountTokenFileName = "token";
    private const string ServiceAccountRootCaFileName = "ca.crt";
    private const string ServiceAccountNamespaceFileName = "namespace";

    private const string ServiceHostEnvironmentVariableName = "KUBERNETES_SERVICE_HOST";
    private const string ServicePortEnvironmentVariableName = "KUBERNETES_SERVICE_PORT";

    private static readonly string ServiceAccountPath =
        Path.Combine(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "C:\\"
                : "/",
            "var",
            "run",
            "secrets",
            "kubernetes.io",
            "serviceaccount");

    /// <summary>
    /// Gets a value indicating whether the current process is running inside a Kubernetes cluster.
    /// </summary>
    /// <returns><c>true</c> if the current process is running inside a Kubernetes cluster; otherwise, <c>false</c>.</returns>
    public static bool IsInCluster()
    {
        string? host = Environment.GetEnvironmentVariable(ServiceHostEnvironmentVariableName);
        string? port = Environment.GetEnvironmentVariable(ServicePortEnvironmentVariableName);

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(port))
            return false;

        string tokenPath = Path.Combine(ServiceAccountPath, ServiceAccountTokenFileName);
        if (!File.Exists(tokenPath))
            return false;

        string rootCaPath = Path.Combine(ServiceAccountPath, ServiceAccountRootCaFileName);
        if (!File.Exists(rootCaPath))
            return false;

        return true;
    }

    /// <inheritdoc />
    public KubernetesClientOptions CreateOptions()
    {
        var options = new KubernetesClientOptions();
        BindOptions(options);
        return options;
    }

    /// <inheritdoc />
    public virtual void BindOptions(KubernetesClientOptions options)
    {
        Ensure.Arg.NotNull(options);

        if (!IsInCluster())
        {
            throw new KubernetesConfigException(
                $"Unable to load in-cluster configuration. Missing environment variables '{ServiceHostEnvironmentVariableName}' and '{ServicePortEnvironmentVariableName}' or service account token. Hint: consider using option 'automountServiceAccountToken: true' in deployment declaration.");
        }

        string rootCaPath = Path.Combine(ServiceAccountPath, ServiceAccountRootCaFileName);
        string tokenPath = Path.Combine(ServiceAccountPath, ServiceAccountTokenFileName);

        string host = Environment.GetEnvironmentVariable(ServiceHostEnvironmentVariableName) !;
        string port = Environment.GetEnvironmentVariable(ServicePortEnvironmentVariableName) !;

        options.Host = new UriBuilder(Uri.UriSchemeHttps, host, Convert.ToInt32(port)).ToString();
        options.TokenProvider = new ServiceAccountTokenProvider(tokenPath);
        options.CertificateAuthorityFilePath = rootCaPath;

        string namespaceFile = Path.Combine(ServiceAccountPath, ServiceAccountNamespaceFileName);
        if (File.Exists(namespaceFile))
        {
            options.Namespace = File.ReadAllText(namespaceFile);
        }
    }
}