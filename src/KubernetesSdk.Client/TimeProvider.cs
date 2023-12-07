// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Kubernetes.Client;

internal static class TimeProvider
{
    private static readonly ITimeProvider Default = new DefaultTimeProvider();
    private static readonly AsyncLocal<ITimeProvider?> Current = new ();

    private sealed class DefaultTimeProvider : ITimeProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }

    [NotNull]
    public static ITimeProvider? Instance
    {
        get => Current.Value ?? Default;
        set => Current.Value = value;
    }

    public static DateTimeOffset UtcNow => Instance.UtcNow;
}