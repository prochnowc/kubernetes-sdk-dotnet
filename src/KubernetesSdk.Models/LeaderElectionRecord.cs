// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System;
using System.Text.Json.Serialization;

namespace Kubernetes.Models;

/// <summary>
/// Represents the record that is stored in the leader election annotation.
/// This information should be used for observational purposes only and could be replaced with a
/// random string (e.g. UUID) with only slight modification of this code.
/// </summary>
public class LeaderElectionRecord
{
    /// <summary>
    /// Gets or sets the identity that owns the lease. If empty, no one owns this lease and all callers may acquire.
    /// </summary>
    [JsonPropertyName("holderIdentity")]
    public string? HolderIdentity { get; set; }

    /// <summary>
    /// Gets or sets the lease duration in seconds.
    /// </summary>
    [JsonPropertyName("leaseDurationSeconds")]
    public int LeaseDurationSeconds { get; set; }

    /// <summary>
    /// Gets or sets the lease acquire time.
    /// </summary>
    [JsonPropertyName("acquireTime")]
    public DateTime? AcquireTime { get; set; }

    /// <summary>
    /// Gets or sets the lease renew time.
    /// </summary>
    [JsonPropertyName("renewTime")]
    public DateTime? RenewTime { get; set; }

    /// <summary>
    /// Gets or sets the leader transitions.
    /// </summary>
    [JsonPropertyName("leaderTransitions")]
    public int LeaderTransitions { get; set; }
}