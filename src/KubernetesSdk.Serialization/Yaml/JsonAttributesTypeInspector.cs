// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace Kubernetes.Serialization.Yaml;

/// <summary>
/// Overrides properties from <see cref="JsonIgnoreAttribute"/>, <see cref="JsonPropertyNameAttribute"/> and
/// <see cref="JsonPropertyOrderAttribute"/>.
/// </summary>
public sealed class JsonAttributesTypeInspector : TypeInspectorSkeleton
{
    private readonly ITypeInspector _innerTypeDescriptor;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonAttributesTypeInspector"/> class.
    /// </summary>
    /// <param name="innerTypeDescriptor">The inner <see cref="ITypeInspector"/>.</param>
    public JsonAttributesTypeInspector(ITypeInspector innerTypeDescriptor)
    {
        _innerTypeDescriptor = innerTypeDescriptor;
    }

    /// <inheritdoc/>
    public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
    {
        return _innerTypeDescriptor
               .GetProperties(type, container)
               .Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null)
               .Select(
                   p =>
                   {
                       var property = new PropertyDescriptor(p);

                       var nameAttribute = p.GetCustomAttribute<JsonPropertyNameAttribute>();
                       if (nameAttribute != null)
                       {
                           property.Name = nameAttribute.Name;
                       }

                       var orderAttribute = p.GetCustomAttribute<JsonPropertyOrderAttribute>();
                       if (orderAttribute != null)
                       {
                           property.Order = orderAttribute.Order;
                       }

                       return (IPropertyDescriptor)property;
                   })
               .OrderBy(p => p.Order);
    }
}