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
/// Provides a access token from an external credential process.
/// </summary>
public sealed class ExternalCredentialTokenProvider : TokenProviderBase
{
    private const string TokenTypeTag = "external_credential";

    private readonly TokenProviderMetrics _metrics;
    private readonly ExternalCredentialProcess _process;

    internal ExternalCredentialTokenProvider(ExternalCredentialProcess process, ExecCredential credential)
        : base(credential.Status?.Token, credential.Status?.ExpirationTimestamp)
    {
        _metrics = new TokenProviderMetrics(TokenTypeTag);
        _process = process;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalCredentialTokenProvider"/> class.
    /// </summary>
    /// <param name="credential">The external credential.</param>
    /// <param name="serializerFactory">The <see cref="IKubernetesSerializerFactory"/>.</param>
    public ExternalCredentialTokenProvider(ExternalCredential credential, IKubernetesSerializerFactory serializerFactory)
        : base(null, null)
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
            "RefreshExternalCredentialToken",
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

    /// <inheritdoc />
    protected override async Task<(string token, DateTimeOffset? expires)> RefreshTokenAsync(
        CancellationToken cancellationToken)
    {
        using Activity? activity = StartActivity();

        try
        {
            using TokenProviderMetrics.TrackedRequest trackedRequest = _metrics.TrackRequest();

            ExecCredential credential =
                await _process.ExecuteAsync(TimeSpan.FromMinutes(2), cancellationToken)
                              .ConfigureAwait(false);

            trackedRequest.Complete();

            if (credential.Status?.IsValid() != true)
            {
                throw new KubernetesRequestException(
                    "Received bad response from external command to receive credentials");
            }

            activity?.SetTag(OtelTags.TokenExpiresAt, credential.Status?.ExpirationTimestamp?.ToString("O"));
            activity?.SetStatus(ActivityStatusCode.Ok);

            return (credential.Status!.Token!, credential.Status.ExpirationTimestamp);
        }
        catch (Exception error)
        {
            activity?.SetStatus(ActivityStatusCode.Error, error.Message);
            throw;
        }
    }
}