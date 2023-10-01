using System.Collections.Generic;

namespace Kubernetes.Generator;

internal static class NameTransformer
{
    private static readonly HashSet<string> Keywords = new ()
    {
        "continue",
        "namespace",
        "operator",
        "default",
        "ref",
        "enum",
        "object",
    };

    public static string GetParameterName(string name)
    {
        if (name.StartsWith("$"))
        {
            name = name.Substring(1);
        }

        if (Keywords.Contains(name))
        {
            return "@" + name;
        }

        return name;
    }

    public static string GetPropertyName(string name)
    {
        if (name.StartsWith("$"))
        {
            return name.Substring(1);
        }

        return name;
    }
}
