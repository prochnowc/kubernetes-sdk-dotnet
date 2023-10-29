using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NJsonSchema;

namespace Kubernetes.Generator;

internal static class JsonSchemaExtensions
{
    private const string ExtensionDataKey = "x-kubernetes-group-version-kind";
    private const string GroupKey = "group";
    private const string VersionKey = "version";
    private const string KindKey = "kind";

    public static bool HasKubernetesGroupVersionKind(this IJsonExtensionObject schema)
    {
        Ensure.Arg.NotNull(schema);
        return schema.ExtensionData?.ContainsKey(ExtensionDataKey) == true;
    }

    public static bool TryGetKubernetesGroupVersionKind(
        this IJsonExtensionObject schema,
        [NotNullWhen(true)] out GroupVersionKind? groupVersionKind)
    {
        Ensure.Arg.NotNull(schema);

        if (schema.ExtensionData?.TryGetValue(ExtensionDataKey, out object? value) != true)
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
}
