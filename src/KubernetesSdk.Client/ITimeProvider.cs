// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace Kubernetes.Client;

internal interface ITimeProvider
{
    DateTimeOffset UtcNow { get; }
}