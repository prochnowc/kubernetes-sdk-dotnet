// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Kubernetes.Client.Diagnostics;

namespace Kubernetes.Client.Authentication;

/// <summary>
/// Provides a access token from an OIDC provider.
/// </summary>
public sealed class OidcTokenProvider : TokenProviderBase
{
    private const string TokenTypeTag = "oidc";

    private readonly TokenProviderMetrics _metrics;
    private readonly string _issuerUrl;
    private readonly string _clientId;
    private readonly string? _clientSecret;
    private string _refreshToken;

    public OidcTokenProvider(
        string issuerUrl,
        string clientId,
        string? clientSecret,
        string idToken,
        string refreshToken)
        : base(idToken, GetTokenExpiration(idToken))
    {
        Ensure.Arg.NotEmpty(issuerUrl);
        Ensure.Arg.NotEmpty(clientId);
        Ensure.Arg.NotEmpty(idToken);
        Ensure.Arg.NotEmpty(refreshToken);

        _metrics = new TokenProviderMetrics(TokenTypeTag);
        _issuerUrl = issuerUrl;
        _clientId = clientId;
        _clientSecret = clientSecret;
        _refreshToken = refreshToken;
    }

    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument", Justification = "Passed by caller.")]
    private Activity? StartActivity(
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        return KubernetesClientDefaults.ActivitySource.StartActivity(
            this,
            "RefreshOidcToken",
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

    private static DateTimeOffset? GetTokenExpiration(string? idToken)
    {
        if (string.IsNullOrEmpty(idToken))
            return null;

        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken token = handler.ReadJwtToken(idToken);
        return token.Payload.Expiration == null
            ? null
            : DateTimeOffset.FromUnixTimeSeconds((long)token.Payload.Expiration);
    }

    /// <inheritdoc />
    protected override async Task<(string token, DateTimeOffset? expires)> RefreshTokenAsync(
        CancellationToken cancellationToken)
    {
        using Activity? activity = StartActivity();

        try
        {
            using TokenProviderMetrics.TrackedRequest trackedRequest = _metrics.TrackRequest();

            using var httpClient = new HttpClient();
            using var tokenRequest = new RefreshTokenRequest
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret ?? string.Empty,
                Address = _issuerUrl,
                RefreshToken = _refreshToken,
            };

            TokenResponse tokenResponse = await httpClient.RequestRefreshTokenAsync(tokenRequest, cancellationToken);
            trackedRequest.Complete();

            if (tokenResponse.IsError)
            {
                throw new KubernetesClientException(tokenResponse.ErrorDescription);
            }

            string token = tokenResponse.IdentityToken!;
            DateTimeOffset? tokenExpiresAt = tokenResponse.ExpiresIn <= 0
                ? null
                : DateTimeOffset.UtcNow + TimeSpan.FromSeconds(tokenResponse.ExpiresIn);

            activity?.SetTag(OtelTags.TokenExpiresAt, tokenExpiresAt?.ToString("O"));
            activity?.SetStatus(ActivityStatusCode.Ok);

            _refreshToken = tokenResponse.RefreshToken!;
            return (token, tokenExpiresAt);
        }
        catch (Exception error)
        {
            activity?.SetStatus(ActivityStatusCode.Error, error.Message);
            throw;
        }
    }
}