using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Kubernetes.Client.Authentication;
using Kubernetes.Client.KubeConfig;
using Kubernetes.KubeConfig;
using Kubernetes.KubeConfig.Models;
using Kubernetes.Serialization;

namespace Kubernetes.Client;

/// <summary>
/// Populates <see cref="KubernetesClientOptions"/> from kubeconfig file.
/// </summary>
public class KubeConfigOptionsProvider : IKubernetesClientOptionsProvider
{
    /// <summary>
    /// Gets the auth provider option binders.
    /// </summary>
    protected IEnumerable<IAuthProviderOptionsBinder> AuthProviderOptionBinders { get; }

    /// <summary>
    /// Gets the serializer provider.
    /// </summary>
    protected IKubernetesSerializerFactory SerializerFactory { get; }

    /// <summary>
    /// Gets or sets the path of the kubeconfig file to load.
    /// If <c>null</c> the default path will be used.
    /// </summary>
    public string? ConfigPath { get; set; }

    /// <summary>
    /// Gets or sets the name of the context that will be used.
    /// If <c>null</c> or empty the current context will be used.
    /// </summary>
    public string? Context { get; set; }

    public KubeConfigOptionsProvider(
        IEnumerable<IAuthProviderOptionsBinder> authProviderOptionBinders,
        IKubernetesSerializerFactory serializerFactory)
    {
        AuthProviderOptionBinders = authProviderOptionBinders;
        SerializerFactory = serializerFactory;
    }

    public KubernetesClientOptions CreateOptions()
    {
        var options = new KubernetesClientOptions();
        BindOptions(options);
        return options;
    }

    public virtual void BindOptions(KubernetesClientOptions options)
    {
        string configPath = ConfigPath ?? KubeConfigLoader.GetKubeConfigPath();
        V1Config config = LoadConfig(configPath);

        string? contextName = Context ?? config.CurrentContext;
        if (!string.IsNullOrWhiteSpace(contextName))
        {
            BindOptionsFromContext(options, config, contextName!, configPath);
        }

        /*TODO:
        if (!string.IsNullOrWhiteSpace(masterUrl))
        {
            k8SConfiguration.Host = masterUrl;
        }
        */

        if (string.IsNullOrWhiteSpace(options.Host))
        {
            throw new InvalidOperationException("Cannot infer Kubernetes host URL either from context or masterUrl");
        }
    }

    protected virtual V1Config LoadConfig(string configPath)
    {
        var configLoader = new KubeConfigLoader(SerializerFactory);
        return configLoader.Load(configPath);
    }

    private void BindOptionsFromContext(
        KubernetesClientOptions options,
        V1Config config,
        string contextName,
        string configPath)
    {
        Context? context = config.Contexts.FirstOrDefault(
            c => string.Equals(c.Name, contextName, StringComparison.OrdinalIgnoreCase));

        if (context == null)
        {
            throw new InvalidOperationException($"Context '{contextName}' not found in '{configPath}'");
        }

        options.Namespace = context.ContextDetails?.Namespace;

        BindOptionsFromCluster(options, config, context, configPath);
        BindOptionsFromUser(options, config, context, configPath);
    }

    private void BindOptionsFromCluster(
        KubernetesClientOptions options,
        V1Config config,
        Context context,
        string configPath)
    {
        string? clusterName = context.ContextDetails?.Cluster;
        if (string.IsNullOrWhiteSpace(clusterName))
        {
            throw new InvalidOperationException($"Cluster not set for context '{context.Name}' in '{configPath}'");
        }

        Cluster? cluster = config.Clusters.FirstOrDefault(
            c => string.Equals(c.Name, clusterName, StringComparison.OrdinalIgnoreCase));

        if (cluster?.ClusterEndpoint == null)
        {
            throw new InvalidOperationException($"Cluster '{clusterName}' not found in '{configPath}'");
        }

        if (string.IsNullOrWhiteSpace(cluster.ClusterEndpoint?.Server))
        {
            throw new InvalidOperationException($"Server not set for cluster '{clusterName}' in '{configPath}'");
        }

        options.Host = ParseServerUri(cluster.ClusterEndpoint!.Server!, clusterName!, configPath);
        options.SkipTlsVerify = cluster.ClusterEndpoint.SkipTlsVerify;
        options.TlsServerName = cluster.ClusterEndpoint.TlsServerName;

        if (options.Host.StartsWith(Uri.UriSchemeHttps))
        {
            if (!string.IsNullOrEmpty(cluster.ClusterEndpoint.CertificateAuthorityData))
            {
                // This null password is to change the constructor to fix this KB:
                // https://support.microsoft.com/en-us/topic/kb5025823-change-in-how-net-applications-import-x-509-certificates-bf81c936-af2b-446e-9f7a-016f4713b46b
                string? nullPassword = null;
                string data = cluster.ClusterEndpoint.CertificateAuthorityData!;
                var certificate = new X509Certificate2(Convert.FromBase64String(data), nullPassword);
                options.SslCaCerts = new X509Certificate2Collection(certificate);
            }
            else if (!string.IsNullOrEmpty(cluster.ClusterEndpoint.CertificateAuthority))
            {
                var certificate = new X509Certificate2(
                    GetFullPath(
                        configPath,
                        cluster.ClusterEndpoint.CertificateAuthority!));
                options.SslCaCerts = new X509Certificate2Collection(certificate);
            }
        }
    }

    private void BindOptionsFromUser(
        KubernetesClientOptions options,
        V1Config config,
        Context context,
        string configPath)
    {
        if (string.IsNullOrWhiteSpace(context.ContextDetails?.User))
            return;

        User? user = config.Users.FirstOrDefault(
            c => string.Equals(
                c.Name,
                context.ContextDetails!.User,
                StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            throw new InvalidOperationException($"User not found for context '{context.Name}' in '{configPath}'");
        }

        if (user.UserCredentials == null)
        {
            throw new InvalidOperationException($"User credentials not found for user '{user.Name}' in '{configPath}'");
        }

        bool credentialsFound = false;

        // Basic and bearer tokens are mutually exclusive
        if (!string.IsNullOrWhiteSpace(user.UserCredentials.Token))
        {
            options.AccessToken = user.UserCredentials.Token;
            credentialsFound = true;
        }
        else if (!string.IsNullOrWhiteSpace(user.UserCredentials.UserName) &&
                 !string.IsNullOrWhiteSpace(user.UserCredentials.Password))
        {
            options.Username = user.UserCredentials.UserName;
            options.Password = user.UserCredentials.Password;
            credentialsFound = true;
        }

        // Token and cert based auth can co-exist
        if (!string.IsNullOrWhiteSpace(user.UserCredentials.ClientCertificateData) &&
            !string.IsNullOrWhiteSpace(user.UserCredentials.ClientKeyData))
        {
            options.ClientCertificateData = user.UserCredentials.ClientCertificateData;
            options.ClientCertificateKeyData = user.UserCredentials.ClientKeyData;
            credentialsFound = true;
        }

        if (!string.IsNullOrWhiteSpace(user.UserCredentials.ClientCertificate) &&
            !string.IsNullOrWhiteSpace(user.UserCredentials.ClientKey))
        {
            options.ClientCertificateFilePath = GetFullPath(configPath, user.UserCredentials.ClientCertificate!);
            options.ClientKeyFilePath = GetFullPath(configPath, user.UserCredentials.ClientKey!);
            credentialsFound = true;
        }

        string? authProviderName = user.UserCredentials.AuthProvider?.Name;
        if (!string.IsNullOrWhiteSpace(authProviderName))
        {
            IAuthProviderOptionsBinder? optionsResolver = AuthProviderOptionBinders.FirstOrDefault(
                r => string.Equals(r.ProviderName, authProviderName, StringComparison.OrdinalIgnoreCase));

            if (optionsResolver == null)
            {
                throw new InvalidOperationException(
                    $"Unsupported authentication provider '{authProviderName}' for user '{user.Name}' in '{configPath}'");
            }

            credentialsFound = true;
        }

        if (user.UserCredentials.Execute != null)
        {
            if (string.IsNullOrWhiteSpace(user.UserCredentials.Execute.Command))
            {
                throw new InvalidOperationException(
                    $"External command to receive credentials for user '{user.Name}' must include a command to execute in '{configPath}'");
            }

            if (string.IsNullOrWhiteSpace(user.UserCredentials.Execute.ApiVersion))
            {
                throw new InvalidOperationException(
                    $"External command to receive credentials for user '{user.Name}' is missing 'ApiVersion' in '{configPath}'");
            }

            var externalCredentialProcess =
                new ExternalCredentialProcess(user.UserCredentials.Execute, SerializerFactory);

            ExecCredentialsResponse credentialsResponse = externalCredentialProcess.Execute(TimeSpan.FromMinutes(2));

            if (credentialsResponse.Status?.IsValid() != true)
            {
                throw new InvalidOperationException(
                    $"Received bas response from external command to receive credentials for user '{user.Name}'");
            }

            // When reading ClientCertificateData from a config file it will be base64 encoded, and code later in the system (see CertUtils.GeneratePfx)
            // expects ClientCertificateData and ClientCertificateKeyData to be base64 encoded because of this. However the string returned by external
            // auth providers is the raw certificate and key PEM text, so we need to take that and base64 encoded it here so it can be decoded later.
            options.ClientCertificateData = credentialsResponse.Status.ClientCertificateData == null
                ? null
                : Convert.ToBase64String(Encoding.ASCII.GetBytes(credentialsResponse.Status.ClientCertificateData));

            options.ClientCertificateKeyData = credentialsResponse.Status.ClientKeyData == null
                ? null
                : Convert.ToBase64String(Encoding.ASCII.GetBytes(credentialsResponse.Status.ClientKeyData));

            credentialsFound = true;

            // TODO: support client certificates here too.
            options.TokenProvider = new ExternalCredentialTokenProvider(externalCredentialProcess, credentialsResponse);
        }

        if (!credentialsFound)
        {
            throw new InvalidOperationException(
                $"User '{user.Name}' does not have appropriate authentication credentials in '{configPath}'");
        }
    }

    private static string ParseServerUri(string server, string clusterName, string configPath)
    {
        if (!Uri.TryCreate(server, UriKind.Absolute, out Uri? uri))
        {
            throw new InvalidOperationException($"Bad server URL '{server}' for cluster '{clusterName}' in '{configPath}'");
        }

        if (IPAddress.TryParse(uri.Host, out IPAddress? ipAddress))
        {
            if (IPAddress.Equals(IPAddress.Any, ipAddress))
            {
                var builder = new UriBuilder(server)
                {
                    Host = $"{IPAddress.Loopback}",
                };

                server = builder.ToString();
            }
            else if (IPAddress.Equals(IPAddress.IPv6Any, ipAddress))
            {
                var builder = new UriBuilder(server)
                {
                    Host = $"{IPAddress.IPv6Loopback}",
                };

                server = builder.ToString();
            }
        }

        return server;
    }

    private static string GetFullPath(string configPath, string path)
    {
        if (Path.IsPathRooted(path))
        {
            return path;
        }

        return Path.Combine(Path.GetDirectoryName(configPath) !, path);
    }
}