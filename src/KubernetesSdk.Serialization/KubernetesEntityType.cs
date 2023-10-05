// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System;
using AppCore.Diagnostics;
using Kubernetes.Models;

namespace Kubernetes.Serialization;

public sealed class KubernetesEntityType
{
    public Type Type { get; }

    public string Group { get; }

    public string Version { get; }

    public string Kind { get; }

    // TODO: revisit
    public string ListKind { get; }

    // TODO: revisit
    public string Singular { get; }

    // TODO: revisit
    public string Plural { get; }

    internal KubernetesEntityType(
        Type type,
        string kind,
        string listKind,
        string group,
        string version,
        string singular,
        string plural)
    {
        Type = type;
        Group = group;
        Version = version;
        Kind = kind;
        ListKind = listKind;
        Singular = singular;
        Plural = plural;
    }

    public static KubernetesEntityType FromType(Type resourceType)
    {
        Ensure.Arg.NotNull(resourceType);
        return KubernetesEntityTypeCache.Get(resourceType);
    }

    public static KubernetesEntityType FromType<T>()
        where T : class, IKubernetesObject
    {
        return FromType(typeof(T));
    }

    public static KubernetesEntityType FromObject(IKubernetesObject obj)
    {
        Ensure.Arg.NotNull(obj);
        return FromType(obj.GetType());
    }
}