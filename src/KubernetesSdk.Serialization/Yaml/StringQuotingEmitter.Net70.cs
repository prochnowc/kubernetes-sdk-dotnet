using System.Text.RegularExpressions;

#if NET7_0_OR_GREATER

namespace Kubernetes.Serialization.Yaml;

public partial class StringQuotingEmitter
{
    [GeneratedRegex(QuotedRegexPattern, RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex QuotedRegex();
}

#endif
