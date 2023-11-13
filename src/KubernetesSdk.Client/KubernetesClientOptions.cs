// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using Kubernetes.Client.Authentication;
using Polly.Retry;

namespace Kubernetes.Client
{
    /// <summary>
    /// Represents a set of kubernetes client configuration settings.
    /// </summary>
    public class KubernetesClientOptions
    {
        /// <summary>
        /// Gets or sets the current namespace.
        /// </summary>
        public string? Namespace { get; set; }

        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        public string? Host { get; set; }

        /// <summary>
        /// Gets or sets the CA certificates.
        /// </summary>
        public X509Certificate2Collection? CaCerts { get; set; }

        /// <summary>
        /// Gets or sets the client certificate data.
        /// </summary>
        public string? ClientCertificateData { get; set; }

        /// <summary>
        /// Gets or sets the client certificate key.
        /// </summary>
        public string? ClientCertificateKeyData { get; set; }

        /// <summary>
        /// Gets or sets the client certificate filename.
        /// </summary>
        public string? ClientCertificateFilePath { get; set; }

        /// <summary>
        /// Gets or sets the ClientCertificate KeyStoreFlags to specify where and how to import the certificate private key.
        /// </summary>
        public X509KeyStorageFlags? ClientCertificateKeyStoreFlags { get; set; }

        /// <summary>
        /// Gets or sets the client certificate key filename.
        /// </summary>
        public string? ClientKeyFilePath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to skip TLS server certificate validation.
        /// </summary>
        public bool SkipTlsVerify { get; set; }

        /// <summary>
        /// Gets or sets the TLS server name. This is used to override the TLS server name.
        /// </summary>
        public string? TlsServerName { get; set; }

        /// <summary>
        /// Gets or sets the HTTP user agent.
        /// </summary>
        public ProductInfoHeaderValue? UserAgent { get; set; }

        /// <summary>
        /// Gets or sets the username (HTTP basic authentication).
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets the password (HTTP basic authentication).
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the access token for OAuth2 authentication.
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ITokenProvider"/> for authentication.
        /// </summary>
        public ITokenProvider? TokenProvider { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to not use HTTP/2 even it is available.
        /// </summary>
        public bool DisableHttp2 { get; set; } = false;

        /// <summary>
        /// Gets or sets the timeout of REST calls to Kubernetes server.
        /// </summary>
        /// <remarks>
        /// Does not apply to watch related API.
        /// </remarks>
        public TimeSpan HttpClientTimeout { get; set; } = TimeSpan.FromSeconds(100);

        /// <summary>
        /// Gets or sets the policy for retrying HTTP requests.
        /// </summary>
        public AsyncRetryPolicy<HttpResponseMessage>? HttpClientRetryPolicy { get; set; }
    }
}