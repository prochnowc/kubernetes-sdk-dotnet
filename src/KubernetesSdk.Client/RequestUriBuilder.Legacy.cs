// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

#if !NET7_0_OR_GREATER

using System;
using System.Text.RegularExpressions;

namespace Kubernetes.Client;

/// <content>
/// Legacy implementation of <see cref="RequestUriBuilder"/>.
/// </content>
internal partial class RequestUriBuilder
{
    private static readonly Regex PathParametersRegexLegacy = new (
        PathParametersRegexPattern,
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(1000));

    private static Regex PathParametersRegex() => PathParametersRegexLegacy;
}

#endif
