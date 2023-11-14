// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Client.Authentication;

namespace Kubernetes.Client.Http;

/// <summary>
/// Provides <see cref="HttpClient"/> authentication using bearer token authentication.
/// </summary>
public sealed class TokenAuthenticationHandler : DelegatingHandler
{
    private const string AuthenticationScheme = "Bearer";
    private readonly ITokenProvider _tokenProvider;

    /// <summary>
    /// Used for statically defined AccessToken via KubernetesClientOptions.
    /// </summary>
    private sealed class ConstantTokenProvider : ITokenProvider
    {
        private readonly string _token;

        public ConstantTokenProvider(string token)
        {
            Ensure.Arg.NotEmpty(token);
            _token = token;
        }

        public Task<string> GetTokenAsync(bool forceRefresh, CancellationToken cancellationToken)
        {
            return Task.FromResult(_token);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenAuthenticationHandler"/> class.
    /// </summary>
    /// <param name="tokenProvider">The <see cref="ITokenProvider"/> used to obtain tokens.</param>
    public TokenAuthenticationHandler(ITokenProvider tokenProvider)
    {
        Ensure.Arg.NotNull(tokenProvider);
        _tokenProvider = tokenProvider;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenAuthenticationHandler"/> class.
    /// </summary>
    /// <param name="token">The token to use for authentication.</param>
    public TokenAuthenticationHandler(string token)
        : this(new ConstantTokenProvider(token))
    {
    }

    private async Task<HttpResponseMessage> SendAuthenticatedAsync(
        HttpRequestMessage request,
        bool forceRefresh,
        CancellationToken cancellationToken)
    {
        string authenticationParameter = await _tokenProvider.GetTokenAsync(forceRefresh, cancellationToken)
                                                             .ConfigureAwait(false);

        request.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationScheme, authenticationParameter);

        return await base.SendAsync(request, cancellationToken)
                         .ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await SendAuthenticatedAsync(request, false, cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            response.Dispose();
            response = await SendAuthenticatedAsync(request, true, cancellationToken)
                .ConfigureAwait(false);
        }

        return response;
    }
}