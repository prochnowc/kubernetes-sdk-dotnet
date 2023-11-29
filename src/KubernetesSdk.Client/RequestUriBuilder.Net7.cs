// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

#if NET7_0_OR_GREATER

using System.Text.RegularExpressions;

namespace Kubernetes.Client;

/// <content>
/// .NET 7 implementation of <see cref="RequestUriBuilder"/>.
/// </content>
internal partial class RequestUriBuilder
{
    [GeneratedRegex(PathParametersRegexPattern, RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PathParametersRegex();
}

#endif
