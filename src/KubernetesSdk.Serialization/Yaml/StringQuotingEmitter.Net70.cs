// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

#if NET7_0_OR_GREATER

using System.Text.RegularExpressions;

namespace Kubernetes.Serialization.Yaml;

/// <content>
/// .NET 7 specific implementation of <see cref="StringQuotingEmitter"/>.
/// </content>
public partial class StringQuotingEmitter
{
    [GeneratedRegex(QuotedRegexPattern, RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex QuotedRegex();
}

#endif
