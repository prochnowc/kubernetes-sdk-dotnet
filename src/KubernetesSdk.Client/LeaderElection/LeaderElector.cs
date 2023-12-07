// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Models;

namespace Kubernetes.Client.LeaderElection;

/// <summary>
/// Provides a leader election mechanism.
/// </summary>
public class LeaderElector
{
    private readonly KubernetesClient _client;
    private readonly LeaderElectorOptions _options;
    private volatile LeaderElectionRecord? _observedRecord;
    private string? _reportedLeader;

    private ILock Lock => _options.Lock!;

    /// <summary>
    /// Gets a value indicating whether this <see cref="LeaderElector"/> is the leader.
    /// </summary>
    public bool IsLeader =>
        !string.IsNullOrWhiteSpace(_observedRecord?.HolderIdentity)
        && _observedRecord.HolderIdentity == Lock.Identity;

    /// <summary>
    /// Gets the identity of the current leader.
    /// </summary>
    public string? LeaderIdentity
            => _observedRecord?.HolderIdentity;

    /// <summary>
    /// Invoked when the current leader changes.
    /// </summary>
    public event Func<string, Task>? LeaderChanged;

    /// <summary>
    /// Invoked when the <see cref="LeaderElector"/> starts leading.
    /// </summary>
    public event Func<Task>? StartedLeading;

    /// <summary>
    /// Invoked when the <see cref="LeaderElector"/> stops leading.
    /// </summary>
    public event Func<Task>? StoppedLeading;

    /// <summary>
    /// Initializes a new instance of the <see cref="LeaderElector"/> class.
    /// </summary>
    /// <param name="client">The <see cref="KubernetesClient"/>.</param>
    /// <param name="options">The <see cref="LeaderElectorOptions"/>.</param>
    public LeaderElector(KubernetesClient client, LeaderElectorOptions options)
    {
        Ensure.Arg.NotNull(client);
        Ensure.Arg.NotNull(options);

        options = options.Seal();
        options.Validate();

        _client = client;
        _options = options;
    }

    /// <summary>
    /// Starts the leader election loop.
    /// </summary>
    /// <param name="cancellationToken">When signaled the leader election loop will stop.</param>
    /// <returns>The asynchronous task.</returns>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                bool acquired = false;
                do
                {
                    double retryDelay = _options.RetryPeriod.TotalMilliseconds
                                        * 1.2 * (ConcurrentRandom.NextDouble() + 1);

                    await Task.Delay((int)retryDelay, cancellationToken)
                              .ConfigureAwait(false);

                    try
                    {
                        acquired = await TryAcquireAsync(cancellationToken)
                            .ConfigureAwait(false);

                        await MaybeReportTransitionAsync()
                            .ConfigureAwait(false);
                    }
                    catch (KubernetesRequestException)
                    {
                        // ignore
                    }
                }
                while (!acquired);

                await OnStartedLeadingAsync()
                    .ConfigureAwait(false);

                bool renewed;
                do
                {
                    await Task.Delay(_options.RenewDeadline, cancellationToken)
                              .ConfigureAwait(false);

                    try
                    {
                        renewed = await TryRenewAsync(cancellationToken)
                            .ConfigureAwait(false);
                    }
                    catch (KubernetesRequestException)
                    {
                        renewed = false;
                    }
                }
                while (renewed);

                await OnStoppedLeadingAsync()
                    .ConfigureAwait(false);
            }
            while (true);
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        finally
        {
            bool leader = IsLeader;
            _observedRecord = null;

            if (leader)
            {
                await OnStoppedLeadingAsync()
                    .ConfigureAwait(false);
            }
        }
    }

    private async Task<bool> TryAcquireAsync(CancellationToken cancellationToken)
    {
        LeaderElectionRecord? leaderElectionRecord = null;
        try
        {
            leaderElectionRecord = await Lock.GetAsync(_client, cancellationToken)
                                             .ConfigureAwait(false);
        }
        catch (KubernetesRequestException error)
        {
            if (error.Status?.Code != (int)HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        DateTimeOffset currentTime = TimeProvider.UtcNow;

        if (leaderElectionRecord?.AcquireTime == null
            || leaderElectionRecord.RenewTime == null
            || leaderElectionRecord.HolderIdentity == null)
        {
            leaderElectionRecord = new LeaderElectionRecord
            {
                HolderIdentity = Lock.Identity,
                LeaseDurationSeconds = (int)_options.LeaseDuration.TotalSeconds,
                AcquireTime = currentTime.UtcDateTime,
                RenewTime = currentTime.UtcDateTime,
                LeaderTransitions = 0,
            };

            if (await Lock.CreateAsync(_client, leaderElectionRecord, cancellationToken)
                          .ConfigureAwait(false))
            {
                _observedRecord = leaderElectionRecord;
                return true;
            }

            return false;
        }

        currentTime = TimeProvider.UtcNow;

        if (!CompareLeaderElectionRecord(_observedRecord, leaderElectionRecord))
        {
            _observedRecord = leaderElectionRecord;
        }

        bool leader = !string.IsNullOrEmpty(leaderElectionRecord.HolderIdentity)
                      && leaderElectionRecord.HolderIdentity == Lock.Identity;

        if (!leader && leaderElectionRecord.RenewTime + _options.LeaseDuration > currentTime)
        {
            return false;
        }

        leaderElectionRecord.HolderIdentity = Lock.Identity;
        leaderElectionRecord.LeaseDurationSeconds = (int)_options.LeaseDuration.TotalSeconds;
        leaderElectionRecord.AcquireTime = currentTime.UtcDateTime;
        leaderElectionRecord.RenewTime = currentTime.UtcDateTime;
        leaderElectionRecord.LeaderTransitions += 1;

        if (!await Lock.UpdateAsync(_client, leaderElectionRecord, cancellationToken)
                        .ConfigureAwait(false))
        {
            return false;
        }

        _observedRecord = leaderElectionRecord;
        return true;
    }

    private async Task<bool> TryRenewAsync(CancellationToken cancellationToken)
    {
        LeaderElectionRecord leaderElectionRecord = await Lock.GetAsync(_client, cancellationToken)
                                                              .ConfigureAwait(false);

        bool leader = leaderElectionRecord.HolderIdentity == Lock.Identity;
        if (!leader)
        {
            return false;
        }

        leaderElectionRecord.RenewTime = TimeProvider.UtcNow.UtcDateTime;

        if (!await Lock.UpdateAsync(_client, leaderElectionRecord, cancellationToken)
                       .ConfigureAwait(false))
        {
            return false;
        }

        _observedRecord = leaderElectionRecord;
        return true;
    }

    private bool CompareLeaderElectionRecord(LeaderElectionRecord? left, LeaderElectionRecord? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (ReferenceEquals(left, null) && !ReferenceEquals(right, null))
        {
            return false;
        }

        return left!.HolderIdentity == right!.HolderIdentity
               && Nullable.Equals(left.AcquireTime, right.AcquireTime)
               && Nullable.Equals(left.RenewTime, right.RenewTime);
    }

    private async Task MaybeReportTransitionAsync()
    {
        if (_observedRecord == null)
        {
            return;
        }

        if (_observedRecord.HolderIdentity == _reportedLeader)
        {
            return;
        }

        _reportedLeader = _observedRecord.HolderIdentity;

        await OnLeaderChangedAsync(_reportedLeader!)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Invoked when the leader has changed.
    /// </summary>
    /// <param name="leader">The identity of the leader.</param>
    /// <returns>Asynchronous task.</returns>
    protected virtual async Task OnLeaderChangedAsync(string leader)
    {
        await RaiseEventAsync(LeaderChanged, leader)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Invoked when this instance becomes the leader.
    /// </summary>
    /// <returns>Asynchronous task.</returns>
    protected virtual async Task OnStartedLeadingAsync()
    {
        await RaiseEventAsync(StartedLeading)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Invoked when this instance stops being the leader.
    /// </summary>
    /// <returns>Asynchronous task.</returns>
    protected virtual async Task OnStoppedLeadingAsync()
    {
        await RaiseEventAsync(StoppedLeading)
            .ConfigureAwait(false);
    }

    private async Task RaiseEventAsync(Func<Task>? eventHandler)
    {
        IEnumerable<Delegate> handlers = eventHandler?.GetInvocationList() ?? Enumerable.Empty<Delegate>();

        foreach (Delegate @delegate in handlers)
        {
            var handler = (Func<Task>)@delegate;
            await handler()
                .ConfigureAwait(false);
        }
    }

    private async Task RaiseEventAsync<T>(Func<T, Task>? eventHandler, T arg)
    {
        IEnumerable<Delegate> handlers = eventHandler?.GetInvocationList() ?? Enumerable.Empty<Delegate>();

        foreach (Delegate @delegate in handlers)
        {
            var handler = (Func<T, Task>)@delegate;
            await handler(arg)
                .ConfigureAwait(false);
        }
    }
}