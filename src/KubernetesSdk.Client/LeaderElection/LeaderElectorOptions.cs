// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace Kubernetes.Client.LeaderElection;

/// <summary>
/// Provides options for the <see cref="LeaderElector"/>.
/// </summary>
public class LeaderElectorOptions
{
    private bool _isReadOnly;
    private ILock? _lock;
    private TimeSpan _leaseDuration = TimeSpan.FromSeconds(15);
    private TimeSpan _renewDeadline = TimeSpan.FromSeconds(10);
    private TimeSpan _retryPeriod = TimeSpan.FromSeconds(2);
    private Func<DateTimeOffset> _timeProvider = () => DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the <see cref="ILock"/> used by the <see cref="LeaderElector"/>.
    /// </summary>
    public ILock? Lock
    {
        get => _lock;
        set
        {
            EnsureWritable();
            _lock = value;
        }
    }

    /// <summary>
    /// Gets or sets the duration of a lease before it must be renewed.
    /// </summary>
    /// <remarks>Defaults to 15 seconds.</remarks>
    public TimeSpan LeaseDuration
    {
        get => _leaseDuration;
        set
        {
            EnsureWritable();
            _leaseDuration = value;
        }
    }

    /// <summary>
    /// Gets or sets the duration before the lease is renewed.
    /// </summary>
    /// <remarks>
    /// Defaults to 10 seconds.
    /// </remarks>
    public TimeSpan RenewDeadline
    {
        get => _renewDeadline;
        set
        {
            EnsureWritable();
            _renewDeadline = value;
        }
    }

    /// <summary>
    /// Gets or sets the period between attempts to acquire the lease.
    /// </summary>
    /// <remarks>
    /// Defaults to 2 seconds.
    /// </remarks>
    public TimeSpan RetryPeriod
    {
        get => _retryPeriod;
        set
        {
            EnsureWritable();
            _retryPeriod = value;
        }
    }

    /// <summary>
    /// Gets or sets a delegate to get the current date time in UTC. Used for testing.
    /// </summary>
    internal Func<DateTimeOffset> TimeProvider
    {
        get => _timeProvider;
        set
        {
            EnsureWritable();
            _timeProvider = value;
        }
    }

    /// <summary>
    /// Validates the options.
    /// </summary>
    public void Validate()
    {
        if (Lock == null)
        {
            throw new KubernetesClientException(
                $"Property '{nameof(Lock)}' of '{nameof(LeaderElectorOptions)}' must not be null.");
        }
    }

    /// <summary>
    /// Ensures that the instance is not sealed.
    /// </summary>
    /// <exception cref="InvalidOperationException">The instance is read-only.</exception>
    protected void EnsureWritable()
    {
        if (_isReadOnly)
        {
            throw new InvalidOperationException($"The '{nameof(LeaderElectorOptions)}' instance is read-only.");
        }
    }

    internal LeaderElectorOptions Seal()
    {
        _isReadOnly = true;
        return this;
    }
}