// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Client.KubeConfig;
using Kubernetes.Models.KubeConfig;
using Kubernetes.Serialization;

namespace Kubernetes.Client.Authentication;

/// <summary>
/// Provides a bearer token from an external credential process.
/// </summary>
public sealed class ExternalCredentialTokenProvider : ITokenProvider
{
    private readonly ExternalCredentialProcess _process;
    private ExecCredential? _credential;

    internal ExternalCredentialTokenProvider(ExternalCredentialProcess process, ExecCredential credential)
    {
        _process = process;
        _credential = credential;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalCredentialTokenProvider"/> class.
    /// </summary>
    /// <param name="credential">The external credential.</param>
    /// <param name="serializerFactory">The <see cref="IKubernetesSerializerFactory"/>.</param>
    public ExternalCredentialTokenProvider(ExternalCredential credential, IKubernetesSerializerFactory serializerFactory)
    {
        Ensure.Arg.NotNull(credential);
        Ensure.Arg.NotNull(serializerFactory);

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

    /// <inheritdoc />
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
                $"Received bad response from external command to receive credentials");
        }
    }
}