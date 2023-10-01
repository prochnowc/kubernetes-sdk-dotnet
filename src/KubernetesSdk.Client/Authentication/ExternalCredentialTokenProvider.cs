using System;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.KubeConfig;
using Kubernetes.KubeConfig.Models;
using Kubernetes.Serialization;

namespace Kubernetes.Client.Authentication;

public sealed class ExternalCredentialTokenProvider : ITokenProvider
{
    private readonly ExternalCredentialProcess _process;
    private ExecCredentialsResponse? _response;

    internal ExternalCredentialTokenProvider(ExternalCredentialProcess process, ExecCredentialsResponse response)
    {
        _process = process;
        _response = response;
    }

    public ExternalCredentialTokenProvider(ExternalCredential credential, IKubernetesSerializerFactory serializerFactory)
    {
        _process = new ExternalCredentialProcess(credential, serializerFactory);
    }

    private bool NeedsRefresh()
    {
        if (_response?.Status == null)
            return true;

        if (_response.Status.ExpirationTimestamp == null)
            return false;

        return DateTime.UtcNow.AddSeconds(30) > _response.Status.ExpirationTimestamp;
    }

    public async Task<string> GetTokenAsync(bool forceRefresh, CancellationToken cancellationToken)
    {
        if (forceRefresh || NeedsRefresh())
        {
            await RefreshTokenAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return _response!.Status!.Token!; // already validated
    }

    private async Task RefreshTokenAsync(CancellationToken cancellationToken)
    {
        _response = await _process.ExecuteAsync(TimeSpan.FromMinutes(2), cancellationToken)
                                  .ConfigureAwait(false);

        if (_response.Status?.IsValid() != true)
        {
            throw new InvalidOperationException(
                $"Received bas response from external command to receive credentials");
        }
    }
}