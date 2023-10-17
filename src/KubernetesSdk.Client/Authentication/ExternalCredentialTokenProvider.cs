using System;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Client.KubeConfig;
using Kubernetes.Models.KubeConfig;
using Kubernetes.Serialization;

namespace Kubernetes.Client.Authentication;

public sealed class ExternalCredentialTokenProvider : ITokenProvider
{
    private readonly ExternalCredentialProcess _process;
    private ExecCredential? _credential;

    internal ExternalCredentialTokenProvider(ExternalCredentialProcess process, ExecCredential credential)
    {
        _process = process;
        _credential = credential;
    }

    public ExternalCredentialTokenProvider(ExternalCredential credential, IKubernetesSerializerFactory serializerFactory)
    {
        _process = new ExternalCredentialProcess(credential, serializerFactory);
    }

    private bool NeedsRefresh()
    {
        if (_credential?.Status == null)
            return true;

        if (_credential.Status.ExpirationTimestamp == null)
            return false;

        return DateTime.UtcNow.AddSeconds(30) > _credential.Status.ExpirationTimestamp;
    }

    public async Task<string> GetTokenAsync(bool forceRefresh, CancellationToken cancellationToken)
    {
        if (forceRefresh || NeedsRefresh())
        {
            await RefreshTokenAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return _credential!.Status!.Token!; // already validated
    }

    private async Task RefreshTokenAsync(CancellationToken cancellationToken)
    {
        _credential = await _process.ExecuteAsync(TimeSpan.FromMinutes(2), cancellationToken)
                                  .ConfigureAwait(false);

        if (_credential.Status?.IsValid() != true)
        {
            throw new InvalidOperationException(
                $"Received bas response from external command to receive credentials");
        }
    }
}