// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace Kubernetes.Client;

internal static class ConcurrentRandom
{
    private static readonly Random Random = new ();

    public static int Next(int minValue, int maxValue)
    {
        lock (Random)
        {
            return Random.Next(minValue, maxValue);
        }
    }

    public static double NextDouble()
    {
        lock (Random)
        {
            return Random.NextDouble();
        }
    }
}