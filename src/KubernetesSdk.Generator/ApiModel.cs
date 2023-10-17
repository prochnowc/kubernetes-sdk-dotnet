// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Kubernetes.Generator;

internal sealed class ApiModel
{
    public string Name { get; }

    public GroupVersionKind? GroupVersionKind { get; }

    public string Type { get; }

    public IReadOnlyCollection<string> Interfaces { get; }

    public IReadOnlyCollection<ApiModelProperty> Properties { get; }

    public string? Description { get; }

    public ApiModel(
        string name,
        GroupVersionKind? groupVersionKind,
        string type,
        IReadOnlyCollection<string> interfaces,
        IReadOnlyCollection<ApiModelProperty> properties,
        string? description)
    {
        Name = name;
        GroupVersionKind = groupVersionKind;
        Type = type;
        Interfaces = interfaces;
        Properties = properties;
        Description = description;
    }
}
