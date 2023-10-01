namespace Kubernetes.Generator;

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