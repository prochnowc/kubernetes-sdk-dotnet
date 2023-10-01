// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

namespace Kubernetes.Models;

/// <summary>
/// Describes the format of a <see cref="ResourceQuantity"/>.
/// </summary>
public enum ResourceQuantityFormat
{
    /// <summary>
    /// e.g., 12e6
    /// </summary>
    DecimalExponent,

    /// <summary>
    /// e.g., 12Mi (12 * 2^20)
    /// </summary>
    BinarySI,

    /// <summary>
    /// e.g., 12M  (12 * 10^6)
    /// </summary>
    DecimalSI,
}