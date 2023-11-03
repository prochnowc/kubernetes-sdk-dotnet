// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using Kubernetes.Models;

namespace Kubernetes.Client;

/// <summary>
/// Provides extensions for the <see cref="IWatcher{T}"/> interface.
/// </summary>
public static class WatcherExtensions
{
    /// <summary>
    /// Gets an <see cref="IAsyncEnumerable{T}"/> of <see cref="WatchEvent{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IKubernetesObject"/>.</typeparam>
    /// <param name="watcher">The <see cref="IWatcher{T}"/>.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="IAsyncEnumerable{T}"/>.</returns>
    [SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "AsAsyncEnumerable is commonly used.")]
    public static async IAsyncEnumerable<WatchEvent<T>> AsAsyncEnumerable<T>(
        this IWatcher<T> watcher,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where T : IKubernetesObject
    {
        Ensure.Arg.NotNull(watcher);

        while (await watcher.ReadNextAsync(cancellationToken)
                            .ConfigureAwait(false) is { } @event)
        {
            yield return @event;
        }
    }
}