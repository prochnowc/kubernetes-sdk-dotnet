// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Client.Authentication
{
    /// <summary>
    /// Provides bearer authentication tokens from a service account token file.
    /// </summary>
    public sealed class ServiceAccountTokenProvider : ITokenProvider
    {
        private readonly string _tokenFilePath;
        private string? _token;
        private DateTime _tokenExpiresAt;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceAccountTokenProvider"/> class.
        /// </summary>
        /// <param name="tokenFilePath">The path to the service account token file.</param>
        public ServiceAccountTokenProvider(string tokenFilePath)
        {
            Ensure.Arg.NotEmpty(tokenFilePath);
            _tokenFilePath = tokenFilePath;
        }

        private async Task<string> ReadTokenAsync()
        {
            using var reader = new StreamReader(File.OpenRead(_tokenFilePath));
            return (await reader.ReadToEndAsync()
                                .ConfigureAwait(false)).Trim();
        }

        /// <inheritdoc />
        public async Task<string> GetTokenAsync(bool forceRefresh, CancellationToken cancellationToken)
        {
            if (forceRefresh || _tokenExpiresAt < DateTime.UtcNow)
            {
                _token = await ReadTokenAsync()
                    .ConfigureAwait(false);

                // in fact, the token has a expiry of 10 minutes and kubelet
                // refreshes it at 8 minutes of its lifetime.
                _tokenExpiresAt = DateTime.UtcNow.AddMinutes(9);
            }

            return _token!;
        }
    }
}
