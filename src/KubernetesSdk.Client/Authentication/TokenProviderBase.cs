// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Client.Authentication;

/// <summary>
/// Base class for <see cref="ITokenProvider"/> implementation.
/// </summary>
public abstract class TokenProviderBase : ITokenProvider
{
    private static readonly TimeSpan TokenRefreshOffset = TimeSpan.FromSeconds(5);

    private string? _token;
    private DateTimeOffset? _tokenExpiresAt;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenProviderBase"/> class.
    /// </summary>
    /// <param name="token">The current access token. If <c>null</c>, the token will be refreshed immediately.</param>
    /// <param name="tokenExpiresAt">Expiration of the access token. If <c>null</c> the token is assumed to never expire.</param>
    protected TokenProviderBase(string? token, DateTimeOffset? tokenExpiresAt)
    {
        _token = token;
        _tokenExpiresAt = tokenExpiresAt;
    }

    private bool NeedsRefresh()
    {
        return string.IsNullOrEmpty(_token)
               || (_tokenExpiresAt != null
                   && _tokenExpiresAt - TokenRefreshOffset < TimeProvider.UtcNow);
    }

    /// <summary>
    /// Must be implemented to refresh the token.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The token and expiration.</returns>
    protected abstract Task<(string token, DateTimeOffset? expires)> RefreshTokenAsync(
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public async Task<string> GetTokenAsync(bool forceRefresh, CancellationToken cancellationToken = default)
    {
        if (forceRefresh || NeedsRefresh())
        {
            (_token, _tokenExpiresAt) =
                    await RefreshTokenAsync(cancellationToken)
                        .ConfigureAwait(false);
        }

        return _token!;
    }
}