using System;
using System.Collections.Generic;
using System.Linq;
using CaseExtensions;
using NJsonSchema;
using NSwag;

namespace Kubernetes.Generator;

internal sealed class ApiModelBuilder
{
    private static readonly HashSet<string> ModelFilter = new (StringComparer.OrdinalIgnoreCase)
    {
        "v1.WatchEvent",
    };

    private readonly OpenApiDocument _document;
    private readonly TypeNameResolver _typeNameResolver;

    public ApiModelBuilder(OpenApiDocument document)
    {
        _document = document;
        _typeNameResolver = new TypeNameResolver(document);
    }

    public IEnumerable<ApiModel> GetModels()
    {
        foreach (KeyValuePair<string, JsonSchema> kv in _document.Definitions)
        {
            string name = kv.Key;
            JsonSchema schema = kv.Value;

            if (ModelFilter.Contains(name))
                continue;

            schema.TryGetKubernetesGroupVersionKind(out GroupVersionKind? groupVersionKind);

            var model = new ApiModel(
                name,
                groupVersionKind,
                _typeNameResolver.GetTypeName(schema),
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
                interfaces.Add($"IKubernetesList<{_typeNameResolver.GetTypeName(itemSchema)}>");
                /* } */
            }
            else if (schema.Properties.TryGetValue("metadata", out JsonSchemaProperty? metadataProperty))
            {
                string nullable = metadataProperty.IsRequired
                    ? string.Empty
                    : "?";

                interfaces.Add(
                    $"IKubernetesObject<{_typeNameResolver.GetTypeName(metadataProperty.Reference)}{nullable}>");
            }
            else
            {
                interfaces.Add("IKubernetesObject");
            }

            if (schema.Properties.TryGetValue("spec", out JsonSchemaProperty? specProperty))
            {
                if (specProperty.Reference?.ActualProperties.Any() == true)
                {
                    string nullable = specProperty.IsRequired
                        ? string.Empty
                        : "?";

                    interfaces.Add($"ISpec<{_typeNameResolver.GetTypeName(specProperty.Reference)}{nullable}>");
                }
            }

            if (schema.Properties.TryGetValue("status", out JsonSchemaProperty? statusProperty))
            {
                if (statusProperty.Reference?.ActualProperties.Any() == true)
                {
                    string nullable = statusProperty.IsRequired
                        ? string.Empty
                        : "?";

                    interfaces.Add($"IStatus<{_typeNameResolver.GetTypeName(statusProperty.Reference)}{nullable}>");
                }
            }
        }

        return interfaces;
    }

    private List<ApiModelProperty> GetModelProperties(JsonSchema schema)
    {
        return schema.Properties.Values
                     .OrderBy(p => !p.IsRequired)
                     .Select(
                         p =>
                             new ApiModelProperty(
                                 p.Name,
                                 _typeNameResolver.GetTypeName(p),
                                 NameTransformer.GetPropertyName(p.Name.ToPascalCase()),
                                 NameTransformer.GetParameterName(p.Name.ToCamelCase()),
                                 p.IsRequired,
                                 p.Description))
                     .ToList();
    }
}
