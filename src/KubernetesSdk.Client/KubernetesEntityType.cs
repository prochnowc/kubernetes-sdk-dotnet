// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System;
using Kubernetes.Models;

namespace Kubernetes.Client;

/// <summary>
/// Provides information about a Kubernetes model type decorated with <see cref="KubernetesEntityAttribute"/>.
/// </summary>
public sealed class KubernetesEntityType
{
    /// <summary>
    /// Gets the .NET type of the model type.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets the group the model type belongs to.
    /// </summary>
    public string Group { get; }

    /// <summary>
    /// Gets the API version of the model type.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the named schema the model type is based on.
    /// </summary>
    public string Kind { get; }

    // TODO: revisit
    internal string ListKind { get; }

    // TODO: revisit
    internal string Singular { get; }

    /// <summary>
    /// Gets the plural name of the model type.
    /// </summary>
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

    /// <summary>
    /// Gets the <see cref="KubernetesEntityType"/> for the specified <paramref name="objectType"/>.
    /// </summary>
    /// <param name="objectType">The type of the Kubernetes model.</param>
    /// <returns>The <see cref="KubernetesEntityType"/>.</returns>
    public static KubernetesEntityType FromType(Type objectType)
    {
        Ensure.Arg.NotNull(objectType);
        return KubernetesEntityTypeCache.Get(objectType);
    }

    /// <summary>
    /// Gets the <see cref="KubernetesEntityType"/> for the specified <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the Kubernetes model.</typeparam>
    /// <returns>The <see cref="KubernetesEntityType"/>.</returns>
    public static KubernetesEntityType FromType<T>()
        where T : IKubernetesObject
    {
        return FromType(typeof(T));
    }

    /// <summary>
    /// Gets the <see cref="KubernetesEntityType"/> for the specified <paramref name="obj"/>.
    /// </summary>
    /// <param name="obj">The instance of the Kubernetes model.</param>
    /// <returns>The <see cref="KubernetesEntityType"/>.</returns>
    public static KubernetesEntityType FromObject(IKubernetesObject obj)
    {
        Ensure.Arg.NotNull(obj);
        return FromType(obj.GetType());
    }
}