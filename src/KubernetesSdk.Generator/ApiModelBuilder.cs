using System;
using System.Collections.Generic;
using System.Linq;
using CaseExtensions;
using NJsonSchema;
using NSwag;

namespace Kubernetes.Generator;

internal sealed class ApiModelBuilder
{
    private readonly GeneratorExecutionContext _context;

    private static readonly HashSet<string> ModelFilter = new (StringComparer.OrdinalIgnoreCase)
    {
        "v1.WatchEvent",
    };

    public ApiModelBuilder(GeneratorExecutionContext context)
    {
        _context = context;
    }

    public IEnumerable<ApiModel> GetModels()
    {
        foreach (KeyValuePair<string, JsonSchema> kv in _context.OpenApiDocument.Definitions)
        {
            string name = kv.Key;
            JsonSchema schema = kv.Value;

            if (ModelFilter.Contains(name))
                continue;

            schema.TryGetKubernetesGroupVersionKind(out GroupVersionKind? groupVersionKind);

            var model = new ApiModel(
                name,
                groupVersionKind,
                _context.TypeNameResolver.GetTypeName(schema),
                GetModelInterfaces(schema),
                GetModelProperties(schema),
                schema.Description);

            yield return model;
        }
    }

    private List<string> GetModelInterfaces(JsonSchema schema)
    {
        List<string> interfaces = new ();
        if (schema.HasKubernetesGroupVersionKind())
        {
            if (schema.Properties.TryGetValue("items", out JsonSchemaProperty? itemsProperty))
            {
                JsonSchema itemSchema = itemsProperty.Type == JsonObjectType.Object
                    ? itemsProperty.Reference
                    : itemsProperty.Item.Reference;

                /*
                if (schema.Properties.TryGetValue("metadata", out JsonSchemaProperty? metadataProperty))
                {
                    string nullable = metadataProperty.IsRequired
                        ? string.Empty
                        : "?";

                    interfaces.Add($"IKubernetesList<{_typeNameResolver.GetTypeName(metadataProperty.Reference)}{nullable}, {_typeNameResolver.GetTypeName(itemSchema)}>");
                }
                else
                {*/
                interfaces.Add($"IKubernetesList<{_context.TypeNameResolver.GetTypeName(itemSchema)}>");
                /* } */
            }
            else if (schema.Properties.TryGetValue("metadata", out JsonSchemaProperty? metadataProperty))
            {
                interfaces.Add(
                    $"IKubernetesObject<{_context.TypeNameResolver.GetTypeName(metadataProperty.Reference)}>");
            }
            else
            {
                interfaces.Add("IKubernetesObject");
            }

            if (schema.Properties.TryGetValue("spec", out JsonSchemaProperty? specProperty))
            {
                if (specProperty.Reference?.ActualProperties.Any() == true)
                {
                    interfaces.Add($"ISpec<{_context.TypeNameResolver.GetTypeName(specProperty.Reference)}>");
                }
            }

            if (schema.Properties.TryGetValue("status", out JsonSchemaProperty? statusProperty))
            {
                if (statusProperty.Reference?.ActualProperties.Any() == true)
                {
                    string nullable = statusProperty.IsRequired
                        ? string.Empty
                        : "?";

                    interfaces.Add($"IStatus<{_context.TypeNameResolver.GetTypeName(statusProperty.Reference)}{nullable}>");
                }
            }
        }

        return interfaces;
    }

    private List<ApiModelProperty> GetModelProperties(JsonSchema schema)
    {
        bool IsRequired(JsonSchemaProperty property)
        {
            switch (property.Name)
            {
                case "metadata":
                {
                    string typeName = _context.TypeNameResolver.GetTypeName(property);
                    return typeName is "V1ObjectMeta" or "V1ListMeta";
                }

                case "spec":
                    return true;

                default:
                    return property.IsRequired;
            }
        }

        return schema.Properties.Values
                     .OrderBy(p => !IsRequired(p))
                     .Select(
                         p =>
                             new ApiModelProperty(
                                 p.Name,
                                 _context.TypeNameResolver.GetTypeName(p),
                                 NameTransformer.GetPropertyName(p.Name.ToPascalCase()),
                                 NameTransformer.GetParameterName(p.Name.ToCamelCase()),
                                 IsRequired(p),
                                 p.Description))
                     .ToList();
    }
}
