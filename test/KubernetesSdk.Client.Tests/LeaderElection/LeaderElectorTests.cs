// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Kubernetes.Client.Mock;
using Kubernetes.Models;
using NSubstitute;

#pragma warning disable VSTHRD103 // CancelAsync() Not available in .NET < 8.0

namespace Kubernetes.Client.LeaderElection;

public class LeaderElectorTests
{
    private class TestLock : ILock
    {
        public LeaderElectionRecord? Record { get; set; }

        public string Identity { get; }

        public TestLock(string identity)
        {
            Identity = identity;
        }

        public Task<LeaderElectionRecord> GetAsync(
            KubernetesClient client,
            CancellationToken cancellationToken = default)
        {
            if (Record == null)
            {
                throw new KubernetesRequestException(new V1Status { Code = (int)HttpStatusCode.NotFound });
            }

            return Task.FromResult(Record);
        }

        public Task<bool> CreateAsync(
            KubernetesClient client,
            LeaderElectionRecord record,
            CancellationToken cancellationToken = default)
        {
            if (Record != null)
            {
                return Task.FromResult(false);
            }

            Record = record;
            return Task.FromResult(true);
        }

        public Task<bool> UpdateAsync(
            KubernetesClient client,
            LeaderElectionRecord record,
            CancellationToken cancellationToken = default)
        {
            if (Record == null)
            {
                return Task.FromResult(false);
            }

            Record = record;
            return Task.FromResult(true);
        }

        public string Describe()
        {
            throw new NotImplementedException();
        }
    }

    private readonly TaskCompletionSource<bool> _startedLeadingTcs = new ();
    private readonly TaskCompletionSource<bool> _stoppedLeadingTcs = new ();
    private readonly TaskCompletionSource<bool> _leaderChangedTcs = new ();

    private KubernetesClient Client { get; } = KubernetesTestClient.Create();

    private Func<Task> OnStartedLeading { get; } = Substitute.For<Func<Task>>();

    private Task OnStartedLeadingTask => _startedLeadingTcs.Task;

    private Func<Task> OnStoppedLeading { get; } = Substitute.For<Func<Task>>();

    private Task OnStoppedLeadingTask => _stoppedLeadingTcs.Task;

    private Func<string, Task> OnLeaderChanged { get; } = Substitute.For<Func<string, Task>>();

    private Task OnLeaderChangedTask => _leaderChangedTcs.Task;

    [SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "Should work in this case.")]
    private async Task WaitWithTimeout(Task task, TimeSpan timeout)
    {
        Task result = await Task.WhenAny(task, Task.Delay(timeout));
        if (result != task)
        {
            throw new TimeoutException();
        }
    }

    private LeaderElector CreateElector(KubernetesClient client, LeaderElectorOptions options)
    {
        var elector = new LeaderElector(client, options);

        elector.StartedLeading += async () =>
        {
            await OnStartedLeading();
            _startedLeadingTcs.TrySetResult(true);
        };

        elector.StoppedLeading += async () =>
        {
            await OnStoppedLeading();
            _stoppedLeadingTcs.TrySetResult(true);
        };

        elector.LeaderChanged += async leader =>
        {
            await OnLeaderChanged(leader);
            _leaderChangedTcs.TrySetResult(true);
        };

        return elector;
    }

    [Fact]
    public async Task AcquiresNewLock()
    {
        string identity = "my-identity";
        var @lock = new TestLock(identity);

        DateTimeOffset currentTime = DateTimeOffset.UtcNow;

        var options = new LeaderElectorOptions
        {
            Lock = @lock,
            TimeProvider = () => currentTime,
        };

        LeaderElector elector = CreateElector(Client, options);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        Task electorTask = elector.RunAsync(cts.Token);

        await WaitWithTimeout(OnStartedLeadingTask, options.RetryPeriod + TimeSpan.FromSeconds(3));

        using (new AssertionScope())
        {
            elector.LeaderIdentity.Should()
                   .Be(identity);

            elector.IsLeader.Should()
                   .BeTrue();
        }

        cts.Cancel();
        await WaitWithTimeout(electorTask, TimeSpan.FromSeconds(1));

        using (new AssertionScope())
        {
            @lock.Record.Should()
                 .NotBeNull();

            @lock.Record!.HolderIdentity.Should()
                 .Be(identity);

            @lock.Record.AcquireTime.Should()
                 .Be(currentTime.UtcDateTime);

            @lock.Record.RenewTime.Should()
                 .Be(currentTime.UtcDateTime);

            @lock.Record.LeaseDurationSeconds.Should()
                 .Be((int)options.LeaseDuration.TotalSeconds);

            @lock.Record.LeaderTransitions.Should()
                 .Be(0);
        }

        await OnLeaderChanged.Received(1)
                             .Invoke(identity);

        await OnStartedLeading.Received(1)
                              .Invoke();

        await OnStoppedLeading.Received(1)
                              .Invoke();
    }

    [Fact]
    public async Task RenewsLock()
    {
        string identity = "my-identity";
        var @lock = new TestLock(identity);

        DateTimeOffset currentTime = DateTimeOffset.UtcNow;
        DateTimeOffset initialTime = currentTime;

        var options = new LeaderElectorOptions
        {
            Lock = @lock,
            RenewDeadline = TimeSpan.FromSeconds(2),
            TimeProvider = () => currentTime,
        };

        LeaderElector elector = CreateElector(Client, options);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        Task electorTask = elector.RunAsync(cts.Token);

        await WaitWithTimeout(OnStartedLeadingTask, options.RetryPeriod + TimeSpan.FromSeconds(3));

        currentTime += options.LeaseDuration;

        await Task.Delay(options.RenewDeadline + TimeSpan.FromSeconds(3));

        cts.Cancel();
        await WaitWithTimeout(electorTask, TimeSpan.FromSeconds(1));

        using (new AssertionScope())
        {
            @lock.Record.Should()
                 .NotBeNull();

            @lock.Record!.HolderIdentity.Should()
                 .Be(identity);

            @lock.Record.AcquireTime.Should()
                 .Be(initialTime.UtcDateTime);

            @lock.Record.RenewTime.Should()
                 .Be(currentTime.UtcDateTime);

            @lock.Record.LeaseDurationSeconds.Should()
                 .Be((int)options.LeaseDuration.TotalSeconds);

            @lock.Record.LeaderTransitions.Should()
                 .Be(0);
        }

        await OnLeaderChanged.Received(1)
                             .Invoke(identity);

        await OnStartedLeading.Received(1)
                              .Invoke();

        await OnStoppedLeading.Received(1)
                              .Invoke();
    }

    [Fact]
    public async Task KeepsForeignLock()
    {
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;
        string identity = "my-identity";
        string foreignIdentity = "foreign-identity";

        var record = new LeaderElectionRecord
        {
            HolderIdentity = foreignIdentity,
            AcquireTime = currentTime.UtcDateTime,
            RenewTime = currentTime.UtcDateTime,
            LeaderTransitions = 0,
            LeaseDurationSeconds = 15,
        };

        var @lock = new TestLock(identity)
        {
            Record = record,
        };

        var options = new LeaderElectorOptions
        {
            Lock = @lock,
            TimeProvider = () => currentTime,
        };

        LeaderElector elector = CreateElector(Client, options);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        Task electorTask = elector.RunAsync(cts.Token);

        await WaitWithTimeout(OnLeaderChangedTask, options.RetryPeriod + TimeSpan.FromSeconds(3));

        using (new AssertionScope())
        {
            elector.LeaderIdentity.Should()
                   .Be(foreignIdentity);

            elector.IsLeader.Should()
                   .BeFalse();
        }

        cts.Cancel();
        await WaitWithTimeout(electorTask, TimeSpan.FromSeconds(1));

        await OnLeaderChanged.Received(1)
                             .Invoke(foreignIdentity);

        await OnStartedLeading.DidNotReceive()
                              .Invoke();

        await OnStoppedLeading.DidNotReceive()
                              .Invoke();

        @lock.Record.Should()
             .Be(record);
    }

    [Fact]
    public async Task AcquiresForeignLock()
    {
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;
        string identity = "my-identity";
        string foreignIdentity = "foreign-identity";

        int leaseDurationSeconds = 15;

        var record = new LeaderElectionRecord
        {
            HolderIdentity = foreignIdentity,
            AcquireTime = (currentTime - TimeSpan.FromSeconds(leaseDurationSeconds)).UtcDateTime,
            RenewTime = (currentTime - TimeSpan.FromSeconds(leaseDurationSeconds)).UtcDateTime,
            LeaderTransitions = 0,
            LeaseDurationSeconds = leaseDurationSeconds,
        };

        var @lock = new TestLock(identity)
        {
            Record = record,
        };

        var options = new LeaderElectorOptions
        {
            Lock = @lock,
            TimeProvider = () => currentTime,
        };

        LeaderElector elector = CreateElector(Client, options);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        Task electorTask = elector.RunAsync(cts.Token);

        await WaitWithTimeout(OnStartedLeadingTask, options.RetryPeriod + TimeSpan.FromSeconds(3));

        using (new AssertionScope())
        {
            elector.LeaderIdentity.Should()
                   .Be(identity);

            elector.IsLeader.Should()
                   .BeTrue();
        }

        cts.Cancel();
        await WaitWithTimeout(electorTask, TimeSpan.FromSeconds(1));

        using (new AssertionScope())
        {
            @lock.Record.Should()
                 .NotBeNull();

            @lock.Record!.HolderIdentity.Should()
                 .Be(identity);

            @lock.Record.AcquireTime.Should()
                 .Be(currentTime.UtcDateTime);

            @lock.Record.RenewTime.Should()
                 .Be(currentTime.UtcDateTime);

            @lock.Record.LeaseDurationSeconds.Should()
                 .Be((int)options.LeaseDuration.TotalSeconds);

            @lock.Record.LeaderTransitions.Should()
                 .Be(1);
        }

        await OnLeaderChanged.Received(1)
                             .Invoke(identity);

        await OnStartedLeading.Received(1)
                              .Invoke();

        await OnStoppedLeading.Received(1)
                              .Invoke();
    }
}