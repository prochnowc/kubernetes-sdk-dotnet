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
    protected const string ServiceAccountTokenFileName = "token";
    protected const string ServiceAccountRootCaFileName = "ca.crt";
    protected const string ServiceAccountNamespaceFileName = "namespace";

    protected const string ServiceHostEnvironmentVariableName = "KUBERNETES_SERVICE_HOST";
    protected const string ServicePortEnvironmentVariableName = "KUBERNETES_SERVICE_PORT";

    protected static readonly string ServiceAccountPath =
        Path.Combine(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "C:\\"
                : "/",
            "var",
            "run",
            "secrets",
            "kubernetes.io",
            "serviceaccount");

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

    public KubernetesClientOptions CreateOptions()
    {
        var options = new KubernetesClientOptions();
        BindOptions(options);
        return options;
    }

    public virtual void BindOptions(KubernetesClientOptions options)
    {
        if (!IsInCluster())
        {
            throw new KubernetesConfigException(
                $"Unable to load in-cluster configuration. Missing environment variables {ServiceHostEnvironmentVariableName} and {ServicePortEnvironmentVariableName} or service account token. Hint: consider using option \"automountServiceAccountToken: true\" in deployment declaration.");
        }

        string rootCaPath = Path.Combine(ServiceAccountPath, ServiceAccountRootCaFileName);
        string tokenPath = Path.Combine(ServiceAccountPath, ServiceAccountTokenFileName);

        string host = Environment.GetEnvironmentVariable(ServiceHostEnvironmentVariableName) !;
        string port = Environment.GetEnvironmentVariable(ServicePortEnvironmentVariableName) !;

        options.Host = new UriBuilder("https", host, Convert.ToInt32(port)).ToString();
        options.TokenProvider = new ServiceAccountTokenProvider(tokenPath);
        options.SslCaCerts = CertificateUtils.LoadPem(rootCaPath);

        string namespaceFile = Path.Combine(ServiceAccountPath, ServiceAccountNamespaceFileName);
        if (File.Exists(namespaceFile))
        {
            options.Namespace = File.ReadAllText(namespaceFile);
        }
    }
}