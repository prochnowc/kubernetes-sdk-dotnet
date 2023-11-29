// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Text.RegularExpressions;

#if !NET7_0_OR_GREATER

namespace Kubernetes.Serialization.Yaml;

/// <content>
/// Legacy implementation of <see cref="StringQuotingEmitter"/>.
/// </content>
public partial class StringQuotingEmitter
{
    private static readonly Regex QuotedRegexLegacy = new (
        QuotedRegexPattern,
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(1000));

    private static Regex QuotedRegex() => QuotedRegexLegacy;
}

#endif
