// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Client.Diagnostics;

namespace Kubernetes.Client.Authentication
{
    /// <summary>
    /// Provides bearer authentication tokens from a service account token file.
    /// </summary>
    public sealed class ServiceAccountTokenProvider : ITokenProvider
    {
        private const string TokenTypeTag = "service_account";

        private readonly TokenProviderMetrics _metrics;
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

            _metrics = new TokenProviderMetrics(TokenTypeTag);
            _tokenFilePath = tokenFilePath;
        }

        private async Task<string> ReadTokenAsync()
        {
            using var reader = new StreamReader(File.OpenRead(_tokenFilePath));
            return (await reader.ReadToEndAsync()
                                .ConfigureAwait(false)).Trim();
        }

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument", Justification = "Passed by caller.")]
        private Activity? StartActivity(
            [CallerMemberName] string? memberName = null,
            [CallerFilePath] string? filePath = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            return KubernetesClientDefaults.ActivitySource.StartActivity(
                this,
                "GetServiceAccountToken",
                ActivityKind.Internal,
                () =>
                {
                    TagList tags = default;
                    tags.Add(OtelTags.TokenType, TokenTypeTag);
                    return tags;
                },
                memberName,
                filePath,
                lineNumber);
        }

        /// <inheritdoc />
        public async Task<string> GetTokenAsync(bool forceRefresh, CancellationToken cancellationToken)
        {
            if (forceRefresh || _tokenExpiresAt < DateTime.UtcNow)
            {
                using Activity? activity = StartActivity();

                try
                {
                    using TokenProviderMetrics.TrackedRequest trackedRequest = _metrics.TrackRequest();

                    _token = await ReadTokenAsync()
                        .ConfigureAwait(false);

                    // in fact, the token has a expiry of 10 minutes and kubelet
                    // refreshes it at 8 minutes of its lifetime.
                    _tokenExpiresAt = DateTime.UtcNow.AddMinutes(9);

                    trackedRequest.Complete();

                    activity?.SetTag(OtelTags.TokenExpiresAt, _tokenExpiresAt.ToString("O"));
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
                catch (Exception error)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, error.Message);
                    throw;
                }
            }

            return _token!;
        }
    }
}
