using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NJsonSchema;

namespace Kubernetes.Generator;

internal static class JsonSchemaExtensions
{
    private const string GroupVersionKindExtensionDataKey = "x-kubernetes-group-version-kind";
    private const string ActionExtensionDataKey = "x-kubernetes-action";
    private const string GroupKey = "group";
    private const string VersionKey = "version";
    private const string KindKey = "kind";

    public static bool HasKubernetesGroupVersionKind(this IJsonExtensionObject schema)
    {
        Ensure.Arg.NotNull(schema);
        return schema.ExtensionData?.ContainsKey(GroupVersionKindExtensionDataKey) == true;
    }

    public static bool TryGetKubernetesGroupVersionKind(
        this IJsonExtensionObject schema,
        [NotNullWhen(true)] out GroupVersionKind? groupVersionKind)
    {
        Ensure.Arg.NotNull(schema);

        if (schema.ExtensionData?.TryGetValue(GroupVersionKindExtensionDataKey, out object? value) != true)
        {
            groupVersionKind = null;
            return false;
        }

        var extensionDataDictionary = value as IDictionary<string, object>;
        extensionDataDictionary ??= (IDictionary<string, object>)((object[])value!)[0];
        groupVersionKind = new GroupVersionKind(
            (string)extensionDataDictionary[GroupKey],
            (string)extensionDataDictionary[VersionKey],
            (string)extensionDataDictionary[KindKey]);

        return true;
    }

    public static bool TryGetKubernetesAction(
        this IJsonExtensionObject schema,
        [NotNullWhen(true)] out string? action)
    {
        Ensure.Arg.NotNull(schema);

        if (schema.ExtensionData?.TryGetValue(ActionExtensionDataKey, out object? value) != true)
        {
            action = null;
            return false;
        }

        action = (string?)value;
        return action != null;
    }
}
