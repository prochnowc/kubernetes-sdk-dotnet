// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using Kubernetes.Client.Authentication;
using Kubernetes.Client.Http;
using Polly.Retry;

namespace Kubernetes.Client
{
    /// <summary>
    /// Represents a set of kubernetes client configuration settings.
    /// </summary>
    public class KubernetesClientOptions
    {
        private bool _isReadOnly;
        private string? _namespace;
        private string? _host;
        private X509Certificate2Collection? _caCerts;
        private string? _clientCertificateData;
        private string? _clientCertificateKeyData;
        private string? _clientCertificateFilePath;
        private X509KeyStorageFlags? _clientCertificateKeyStoreFlags;
        private string? _clientKeyFilePath;
        private bool _skipTlsVerify;
        private string? _tlsServerName;
        private ProductInfoHeaderValue? _userAgent;
        private string? _username;
        private string? _password;
        private string? _accessToken;
        private ITokenProvider? _tokenProvider;
        private bool _disableHttp2 = false;
        private TimeSpan _httpClientTimeout = TimeSpan.FromSeconds(100);
        private AsyncRetryPolicy<HttpResponseMessage>? _httpClientRetryPolicy;
        private IList<Func<KubernetesClientOptions, DelegatingHandler>> _httpMessageHandlers =
            new List<Func<KubernetesClientOptions, DelegatingHandler>>
            {
                KubernetesHttpClientFactory.CreateAuthenticationMessageHandler,
            };

        /// <summary>
        /// Gets or sets the current namespace.
        /// </summary>
        public string? Namespace
        {
            get => _namespace;
            set
            {
                EnsureWritable();
                _namespace = value;
            }
        }

        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        public string? Host
        {
            get => _host;
            set
            {
                EnsureWritable();
                _host = value;
            }
        }

        /// <summary>
        /// Gets or sets the CA certificates.
        /// </summary>
        public X509Certificate2Collection? CaCerts
        {
            get => _caCerts;
            set
            {
                EnsureWritable();
                _caCerts = value;
            }
        }

        /// <summary>
        /// Gets or sets the client certificate data.
        /// </summary>
        public string? ClientCertificateData
        {
            get => _clientCertificateData;
            set
            {
                EnsureWritable();
                _clientCertificateData = value;
            }
        }

        /// <summary>
        /// Gets or sets the client certificate key.
        /// </summary>
        public string? ClientCertificateKeyData
        {
            get => _clientCertificateKeyData;
            set
            {
                EnsureWritable();
                _clientCertificateKeyData = value;
            }
        }

        /// <summary>
        /// Gets or sets the client certificate filename.
        /// </summary>
        public string? ClientCertificateFilePath
        {
            get => _clientCertificateFilePath;
            set
            {
                EnsureWritable();
                _clientCertificateFilePath = value;
            }
        }

        /// <summary>
        /// Gets or sets the ClientCertificate KeyStoreFlags to specify where and how to import the certificate private key.
        /// </summary>
        public X509KeyStorageFlags? ClientCertificateKeyStoreFlags
        {
            get => _clientCertificateKeyStoreFlags;
            set
            {
                EnsureWritable();
                _clientCertificateKeyStoreFlags = value;
            }
        }

        /// <summary>
        /// Gets or sets the client certificate key filename.
        /// </summary>
        public string? ClientKeyFilePath
        {
            get => _clientKeyFilePath;
            set
            {
                EnsureWritable();
                _clientKeyFilePath = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to skip TLS server certificate validation.
        /// </summary>
        public bool SkipTlsVerify
        {
            get => _skipTlsVerify;
            set
            {
                EnsureWritable();
                _skipTlsVerify = value;
            }
        }

        /// <summary>
        /// Gets or sets the TLS server name. This is used to override the TLS server name.
        /// </summary>
        public string? TlsServerName
        {
            get => _tlsServerName;
            set
            {
                EnsureWritable();
                _tlsServerName = value;
            }
        }

        /// <summary>
        /// Gets or sets the HTTP user agent.
        /// </summary>
        public ProductInfoHeaderValue? UserAgent
        {
            get => _userAgent;
            set
            {
                EnsureWritable();
                _userAgent = value;
            }
        }

        /// <summary>
        /// Gets or sets the username (HTTP basic authentication).
        /// </summary>
        public string? Username
        {
            get => _username;
            set
            {
                EnsureWritable();
                _username = value;
            }
        }

        /// <summary>
        /// Gets or sets the password (HTTP basic authentication).
        /// </summary>
        public string? Password
        {
            get => _password;
            set
            {
                EnsureWritable();
                _password = value;
            }
        }

        /// <summary>
        /// Gets or sets the access token for OAuth2 authentication.
        /// </summary>
        public string? AccessToken
        {
            get => _accessToken;
            set
            {
                EnsureWritable();
                _accessToken = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ITokenProvider"/> for authentication.
        /// </summary>
        public ITokenProvider? TokenProvider
        {
            get => _tokenProvider;
            set
            {
                EnsureWritable();
                _tokenProvider = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to not use HTTP/2 even it is available.
        /// </summary>
        public bool DisableHttp2
        {
            get => _disableHttp2;
            set
            {
                EnsureWritable();
                _disableHttp2 = value;
            }
        }

        /// <summary>
        /// Gets or sets the timeout of REST calls to Kubernetes server.
        /// </summary>
        /// <remarks>
        /// Does not apply to watch related API.
        /// </remarks>
        public TimeSpan HttpClientTimeout
        {
            get => _httpClientTimeout;
            set
            {
                EnsureWritable();
                _httpClientTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the policy for retrying HTTP requests.
        /// </summary>
        public AsyncRetryPolicy<HttpResponseMessage>? HttpClientRetryPolicy
        {
            get => _httpClientRetryPolicy;
            set
            {
                EnsureWritable();
                _httpClientRetryPolicy = value;
            }
        }

        /// <summary>
        /// Gets the list of HTTP message handler factories.
        /// </summary>
        public IList<Func<KubernetesClientOptions, DelegatingHandler>> HttpMessageHandlers => _httpMessageHandlers;

        /// <summary>
        /// Ensures that the instance is not sealed.
        /// </summary>
        /// <exception cref="InvalidOperationException">The instance is read-only.</exception>
        protected void EnsureWritable()
        {
            if (_isReadOnly)
            {
                throw new InvalidOperationException($"The '{nameof(KubernetesClientOptions)}' instance is read-only.");
            }
        }

        internal KubernetesClientOptions Seal()
        {
            _isReadOnly = true;
            _httpMessageHandlers =
                new ReadOnlyCollection<Func<KubernetesClientOptions, DelegatingHandler>>(_httpMessageHandlers);

            return this;
        }
    }
}