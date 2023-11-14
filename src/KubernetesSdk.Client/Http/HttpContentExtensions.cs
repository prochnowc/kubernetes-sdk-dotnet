// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

#if !NET5_0_OR_GREATER

using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Client;

internal static class HttpContentExtensions
{
    public static async Task<Stream> ReadAsStreamAsync(this HttpContent content, CancellationToken cancellationToken)
    {
        return await content.ReadAsStreamAsync()
                            .ConfigureAwait(false);
    }
}

#endif
