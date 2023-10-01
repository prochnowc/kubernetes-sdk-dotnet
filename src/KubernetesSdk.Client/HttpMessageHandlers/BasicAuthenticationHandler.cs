using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Client.HttpMessageHandlers;

public sealed class BasicAuthenticationHandler : DelegatingHandler
{
    private readonly string _username;
    private readonly string _password;

    public BasicAuthenticationHandler(string username, string password)
    {
        _username = username;
        _password = password;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    string.Format(
                              CultureInfo.InvariantCulture,
                              "{0}:{1}",
                              _username,
                              _password)
                          .ToCharArray())));

        return await base.SendAsync(request, cancellationToken)
                         .ConfigureAwait(false);
    }
}