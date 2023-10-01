using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Client.Authentication
{
    public sealed class ServiceAccountTokenProvider : ITokenProvider
    {
        private readonly string _tokenPath;
        private string? _token;
        private DateTime _tokenExpiresAt;

        public ServiceAccountTokenProvider(string tokenPath)
        {
            _tokenPath = tokenPath;
        }

        private async Task<string> ReadTokenAsync()
        {
            using var reader = new StreamReader(File.OpenRead(_tokenPath));
            return (await reader.ReadToEndAsync()
                                .ConfigureAwait(false)).Trim();
        }

        public async Task<string> GetTokenAsync(bool forceRefresh, CancellationToken cancellationToken)
        {
            if (forceRefresh || _tokenExpiresAt < DateTime.UtcNow)
            {
                _token = await ReadTokenAsync();

                // in fact, the token has a expiry of 10 minutes and kubelet
                // refreshes it at 8 minutes of its lifetime.
                _tokenExpiresAt = DateTime.UtcNow.AddMinutes(9);
            }

            return _token!;
        }
    }
}
