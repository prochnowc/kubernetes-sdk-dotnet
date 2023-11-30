// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

#if !NETSTANDARD2_1_OR_GREATER && !NET5_0_OR_GREATER

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace Kubernetes.Client;

internal static class StreamExtensions
{
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don\'t dispose injected", Justification = "API Shim for missing DisposeAsync()")]
    public static ValueTask DisposeAsync(this Stream stream)
    {
        stream.Dispose();
        return new ();
    }
}

#endif