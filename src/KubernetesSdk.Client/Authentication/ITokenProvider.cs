// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Client.Authentication;

/// <summary>
/// Represents a bearer authentication token provider.
/// </summary>
public interface ITokenProvider
{
    /// <summary>
    /// Gets the bearer authentication token.
    /// </summary>
    /// <param name="forceRefresh">Whether to force a refresh.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The bearer authentication token.</returns>
    Task<string> GetTokenAsync(bool forceRefresh, CancellationToken cancellationToken = default);
}