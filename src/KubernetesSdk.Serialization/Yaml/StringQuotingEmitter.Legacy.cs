using System;
using System.Text.RegularExpressions;

#if !NET7_0_OR_GREATER

namespace Kubernetes.Serialization.Yaml;

public partial class StringQuotingEmitter
{
    private static readonly Regex QuotedRegexLegacy = new (
        QuotedRegexPattern,
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(1000));

    private static Regex QuotedRegex() => QuotedRegexLegacy;
}

#endif
