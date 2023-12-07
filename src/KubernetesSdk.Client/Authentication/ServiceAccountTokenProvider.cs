// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Client.Diagnostics;

namespace Kubernetes.Client.Authentication;

/// <summary>
/// Provides a access token from a service account token file.
/// </summary>
public sealed class ServiceAccountTokenProvider : TokenProviderBase
{
    private const string TokenTypeTag = "service_account";

    // in fact, the token has a expiry of 10 minutes and kubelet
    // refreshes it at 8 minutes of its lifetime.
    internal static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(8);

    private readonly TokenProviderMetrics _metrics;
    private readonly string _tokenFilePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceAccountTokenProvider"/> class.
    /// </summary>
    /// <param name="tokenFilePath">The path to the service account token file.</param>
    public ServiceAccountTokenProvider(string tokenFilePath)
        : base(null, null)
    {
        Ensure.Arg.NotEmpty(tokenFilePath);

        _metrics = new TokenProviderMetrics(TokenTypeTag);
        _tokenFilePath = tokenFilePath;
    }

    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument", Justification = "Passed by caller.")]
    private Activity? StartActivity(
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        return KubernetesClientDefaults.ActivitySource.StartActivity(
            this,
            "RefreshServiceAccountToken",
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
    protected override async Task<(string token, DateTimeOffset? expires)> RefreshTokenAsync(
        CancellationToken cancellationToken)
    {
        using Activity? activity = StartActivity();

        try
        {
            using TokenProviderMetrics.TrackedRequest trackedRequest = _metrics.TrackRequest();

            string token = await FileSystem.ReadAllTextAsync(_tokenFilePath, cancellationToken)
                                           .ConfigureAwait(false);

            trackedRequest.Complete();

            token = token.Trim();
            DateTimeOffset tokenExpiresAt = TimeProvider.UtcNow + TokenLifetime;

            activity?.SetTag(OtelTags.TokenExpiresAt, tokenExpiresAt.ToString("O"));
            activity?.SetStatus(ActivityStatusCode.Ok);

            return (token, tokenExpiresAt);
        }
        catch (Exception error)
        {
            activity?.SetStatus(ActivityStatusCode.Error, error.Message);
            throw;
        }
    }
}