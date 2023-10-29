namespace Kubernetes.Generator;

/// <summary>
/// Holds the 'x-kubernetes-group-version-kind' schema extensions data.
/// </summary>
internal sealed class GroupVersionKind
{
    public string Group { get; }

    public string Version { get; }

    public string Kind { get; }

    public GroupVersionKind(string group, string version, string kind)
    {
        Group = group;
        Version = version;
        Kind = kind;
    }
}