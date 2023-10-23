using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Client.Authentication;

namespace Kubernetes.Client.HttpMessageHandlers;

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

    public TokenAuthenticationHandler(ITokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    public TokenAuthenticationHandler(string token)
        : this(new ConstantTokenProvider(token))
    {
    }

    private async Task<HttpResponseMessage> SendAuthenticatedAsync(
        HttpRequestMessage request,
        bool forceRefresh,
        CancellationToken cancellationToken)
    {
        string token = await _tokenProvider.GetTokenAsync(forceRefresh, cancellationToken)
                                           .ConfigureAwait(false);

        request.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationScheme, token);

        return await base.SendAsync(request, cancellationToken)
                         .ConfigureAwait(false);
    }

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