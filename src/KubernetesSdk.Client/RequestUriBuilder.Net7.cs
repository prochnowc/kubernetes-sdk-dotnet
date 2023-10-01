#if NET7_0_OR_GREATER

using System.Text.RegularExpressions;

namespace Kubernetes.Client;

internal partial class RequestUriBuilder
{
    [GeneratedRegex(PathParametersRegexPattern, RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PathParametersRegex();
}

#endif
