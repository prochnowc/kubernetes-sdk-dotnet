using System;
using System.Collections.Generic;
using System.Linq;
using CaseExtensions;
using NJsonSchema;
using NSwag;

namespace Kubernetes.Generator;

/// <summary>
/// Resolves model type names from the <see cref="OpenApiDocument"/>.
/// </summary>
internal sealed class TypeNameResolver
{
    private readonly Dictionary<string, string> _classNameMap;
    private readonly Dictionary<JsonSchema, string> _schemaToNameMapCooked;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeNameResolver"/> class.
    /// </summary>
    /// <param name="swagger">The <see cref="OpenApiDocument"/>.</param>
    public TypeNameResolver(OpenApiDocument swagger)
    {
        _classNameMap = InitClassNameMap(swagger);
        _schemaToNameMapCooked = GenerateSchemaToNameMapCooked(swagger);
    }

        /// <summary>
    /// Gets the model type name of the specified <see cref="JsonSchema"/>.
    /// </summary>
    /// <param name="definition">The <see cref="JsonSchema"/>.</param>
    /// <returns>The model type name.</returns>
    public string GetTypeName(JsonSchema definition)
    {
        if (definition.TryGetKubernetesGroupVersionKind(out GroupVersionKind? gvk))
        {
            return _classNameMap[$"{gvk.Group}_{gvk.Kind}_{gvk.Version}"];
        }

        return _schemaToNameMapCooked[definition];
    }

    /// <summary>
    /// Gets the model type name of the specified <see cref="OpenApiParameter"/>.
    /// </summary>
    /// <param name="operation">The <see cref="OpenApiOperation"/>.</param>
    /// <param name="parameter">The <see cref="OpenApiParameter"/>.</param>
    /// <returns>The model type name.</returns>
    public string GetParameterTypeName(OpenApiOperation operation, OpenApiParameter parameter)
    {
        string parameterTypeName;

        if (parameter.Schema?.Reference != null)
        {
            parameterTypeName = GetTypeName(parameter.Schema.Reference);
            if (!parameter.IsRequired)
                parameterTypeName += "?";
        }
        else if (parameter.Schema != null)
        {
            parameterTypeName = GetTypeName(
                parameter.Schema.Type,
                parameter.Name,
                parameter.IsRequired,
                parameter.Schema.Format);
        }
        else
        {
            parameterTypeName = GetTypeName(parameter.Type, parameter.Name, parameter.IsRequired, parameter.Format);
        }

        if (string.Equals(parameterTypeName, "object", StringComparison.OrdinalIgnoreCase))
        {
            if (operation.TryGetKubernetesGroupVersionKind(out GroupVersionKind? _)
                || operation.Tags.Contains("custom_objects"))
            {
                parameterTypeName = "IKubernetesObject";
            }
        }

        return parameterTypeName;
    }

    /// <summary>
    /// Gets the model type name of the specified <see cref="OpenApiOperation"/>.
    /// </summary>
    /// <param name="operation">The <see cref="OpenApiOperation"/>.</param>
    /// <returns>The model type name.</returns>
    public string GetResponseTypeName(OpenApiOperation operation)
    {
        string responseTypeName =
            operation.ActualResponses.Select(r => GetTypeName(r.Value))
                     .Distinct()
                     .Single();

        if (string.Equals(responseTypeName, "object", StringComparison.OrdinalIgnoreCase))
        {
            if (operation.TryGetKubernetesGroupVersionKind(out GroupVersionKind? _)
                || operation.Tags.Contains("custom_objects"))
            {
                responseTypeName = "IKubernetesObject";
            }
        }

        return responseTypeName;
    }

    /// <summary>
    /// Gets the model type name of the specified <see cref="JsonSchemaProperty"/>.
    /// </summary>
    /// <param name="p">The <see cref="JsonSchemaProperty"/>.</param>
    /// <returns>The model type name.</returns>
    public string GetTypeName(JsonSchemaProperty p)
    {
        if (p.Reference != null)
        {
            if (p.IsRequired)
            {
                return GetTypeName(p.Reference);
            }
            else
            {
                return GetTypeName(p.Reference) + "?";
            }
        }

        if (p.IsArray)
        {
            // getType
            return $"List<{GetTypeName(p.Item, p)}>" + (p.IsRequired
                ? string.Empty
                : "?");
        }

        if (p.IsDictionary && p.AdditionalPropertiesSchema != null)
        {
            return $"Dictionary<string, {GetTypeName(p.AdditionalPropertiesSchema, p)}>" + (p.IsRequired
                ? string.Empty
                : "?");
        }

        return GetTypeName(p.Type, p.Name, p.IsRequired, p.Format);
    }

    private static Dictionary<JsonSchema, string> GenerateSchemaToNameMapCooked(OpenApiDocument swagger)
    {
        return swagger.Definitions.ToDictionary(
            x => x.Value,
            x => x.Key.Replace(".", string.Empty)
                  .ToPascalCase());
    }

    private Dictionary<string, string> InitClassNameMap(OpenApiDocument doc)
    {
        var map = new Dictionary<string, string>();
        foreach (KeyValuePair<string, JsonSchema> kv in doc.Definitions)
        {
            string? k = kv.Key;
            JsonSchema? v = kv.Value;
            if (v.TryGetKubernetesGroupVersionKind(out GroupVersionKind? gvk))
            {
                map[$"{gvk.Group}_{gvk.Kind}_{gvk.Version}"] = k
                                                               .Replace(".", "_")
                                                               .ToPascalCase();
            }
        }

        return map;
    }

    /*
    public string GetClassName(OpenApiOperation operation)
    {
        var groupVersionKind =
            (Dictionary<string, object>)operation.ExtensionData["x-kubernetes-group-version-kind"];
        return GetClassName(groupVersionKind);
    }*/

    private string GetTypeName(JsonObjectType jsonType, string name, bool required, string format)
    {
        if (name == "pretty" && !required)
        {
            return "bool?";
        }

        switch (jsonType)
        {
            case JsonObjectType.Boolean:
                return required
                    ? "bool"
                    : "bool?";

            case JsonObjectType.Integer:
                switch (format)
                {
                    case "int64":
                        return required
                            ? "long"
                            : "long?";

                    default:
                        return required
                            ? "int"
                            : "int?";
                }

            case JsonObjectType.Number:
                return required
                    ? "double"
                    : "double?";

            case JsonObjectType.String:
                switch (format)
                {
                    case "byte":
                        return required
                            ? "byte[]"
                            : "byte[]?";

                    case "date-time":
                        // eventTime is required but should be optional, see https://github.com/kubernetes-client/csharp/issues/1197
                        if (name == "eventTime")
                        {
                            return "global::System.DateTime?";
                        }

                        return required
                            ? "global::System.DateTime"
                            : "global::System.DateTime?";
                }

                return required
                    ? "string"
                    : "string?";

            case JsonObjectType.Object:
                switch (format)
                {
                    case "file":
                        return "global::System.IO.Stream";
                }

                return required
                    ? "object"
                    : "object?";

            default:
                throw new NotSupportedException();
        }
    }

    private string GetTypeName(OpenApiResponse response)
    {
        if (response.Schema?.Reference != null)
        {
            return GetTypeName(response.Schema.Reference);
        }

        if (response.Schema != null)
        {
            return GetTypeName(
                response.Schema.Type,
                string.Empty,
                true,
                response.Schema.Format);
        }

        return "void";
    }

    private string GetTypeName(JsonSchema? schema, JsonSchemaProperty parent)
    {
        if (schema != null)
        {
            if (schema.IsArray)
            {
                return $"List<{GetTypeName(schema.Item, parent)}>";
            }

            if (schema.IsDictionary && schema.AdditionalPropertiesSchema != null)
            {
                return $"Dictionary<string, {GetTypeName(schema.AdditionalPropertiesSchema, parent)}>";
            }

            if (schema.Reference != null)
            {
                return GetTypeName(schema.Reference);
            }

            // parent.IsRequired??
            return GetTypeName(schema.Type, parent.Name, true, schema.Format);
        }

        return GetTypeName(parent.Type, parent.Name, parent.IsRequired, parent.Format);
    }
}