using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NJsonSchema;

namespace Kubernetes.Generator;

internal static class JsonSchemaExtensions
{
    public static bool HasKubernetesGroupVersionKind(this IJsonExtensionObject schema)
    {
        if (schema == null)
        {
            throw new ArgumentNullException(nameof(schema));
        }

        return schema.ExtensionData?.ContainsKey("x-kubernetes-group-version-kind") == true;
    }

    public static bool TryGetKubernetesGroupVersionKind(
        this IJsonExtensionObject schema,
        [NotNullWhen(true)] out GroupVersionKind? groupVersionKind)
    {
        if (schema == null)
        {
            throw new ArgumentNullException(nameof(schema));
        }

        if (schema.ExtensionData?.TryGetValue("x-kubernetes-group-version-kind", out object? value) != true)
        {
            groupVersionKind = null;
            return false;
        }

        var extensionDataDictionary = value as IDictionary<string, object>;
        extensionDataDictionary ??= (IDictionary<string, object>)((object[])value!)[0];
        groupVersionKind = new GroupVersionKind(
            (string)extensionDataDictionary["group"],
            (string)extensionDataDictionary["version"],
            (string)extensionDataDictionary["kind"]);

        return true;
    }
}
