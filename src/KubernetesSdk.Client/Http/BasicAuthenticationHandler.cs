// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Client.Http;

/// <summary>
/// Provides <see cref="HttpClient"/> authentication using basic authentication (username/password).
/// </summary>
public sealed class BasicAuthenticationHandler : DelegatingHandler
{
    private const string AuthenticationScheme = "Basic";
    private readonly string _username;
    private readonly string _password;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicAuthenticationHandler"/> class.
    /// </summary>
    /// <param name="username">The username used to authenticate.</param>
    /// <param name="password">The password used to authenticate.</param>
    public BasicAuthenticationHandler(string username, string password)
    {
        Ensure.Arg.NotEmpty(username);
        Ensure.Arg.NotNull(password);

        _username = username;
        _password = password;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string authenticationParameter = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(
                FormattableString.Invariant($"{_username}:{_password}")
                                 .ToCharArray()));

        request.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationScheme, authenticationParameter);

        return await base.SendAsync(request, cancellationToken)
                         .ConfigureAwait(false);
    }
}