// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Client.Diagnostics;
using Kubernetes.Client.KubeConfig;
using Kubernetes.Models.KubeConfig;
using Kubernetes.Serialization;

namespace Kubernetes.Client.Authentication;

/// <summary>
/// Provides a bearer token from an external credential process.
/// </summary>
public sealed class ExternalCredentialTokenProvider : ITokenProvider
{
    private const string TokenTypeTag = "external_credential";

    private readonly TokenProviderMetrics _metrics;
    private readonly ExternalCredentialProcess _process;
    private ExecCredential? _credential;

    internal ExternalCredentialTokenProvider(ExternalCredentialProcess process, ExecCredential credential)
    {
        _metrics = new TokenProviderMetrics(TokenTypeTag);
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

        _metrics = new TokenProviderMetrics(TokenTypeTag);
        _process = new ExternalCredentialProcess(credential, serializerFactory);
    }

    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument", Justification = "Passed by caller.")]
    private Activity? StartActivity(
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        return KubernetesClientDefaults.ActivitySource.StartActivity(
            this,
            "GetExternalCredentialToken",
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
            using Activity? activity = StartActivity();

            try
            {
                using TokenProviderMetrics.TrackedRequest trackedRequest = _metrics.TrackRequest();

                bool result = await RefreshTokenAsync(cancellationToken)
                    .ConfigureAwait(false);

                trackedRequest.Complete();

                if (!result)
                {
                    throw new KubernetesRequestException(
                        "Received bad response from external command to receive credentials");
                }

                activity?.SetTag(OtelTags.TokenExpiresAt, _credential?.Status?.ExpirationTimestamp?.ToString("O"));
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception error)
            {
                activity?.SetStatus(ActivityStatusCode.Error, error.Message);
                throw;
            }
        }

        return _credential!.Status!.Token!; // already validated
    }

    private async Task<bool> RefreshTokenAsync(CancellationToken cancellationToken)
    {
        _credential = await _process.ExecuteAsync(TimeSpan.FromMinutes(2), cancellationToken)
                                    .ConfigureAwait(false);

        return _credential.Status?.IsValid() ?? false;
    }
}