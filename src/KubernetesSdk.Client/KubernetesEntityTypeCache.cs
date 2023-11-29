// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using Kubernetes.Models;

namespace Kubernetes.Client;

internal static class KubernetesEntityTypeCache
{
    private static readonly ConcurrentDictionary<Type, KubernetesEntityType> CacheByType = new ();

    public static KubernetesEntityType Get(Type type)
    {
        return CacheByType.GetOrAdd(
            type,
            t =>
            {
                var entityAttribute = t.GetCustomAttribute<KubernetesEntityAttribute>();
                if (entityAttribute == null)
                    throw new ArgumentException("Not a Kubernetes entity.");

                string kind = entityAttribute.Kind;
                string pluralName = string.IsNullOrWhiteSpace(entityAttribute.PluralName)
                    ? $"{kind.ToLower()}s"
                    : entityAttribute.PluralName!;

                return new KubernetesEntityType(
                    t,
                    kind,
                    $"{kind}List",
                    entityAttribute.Group,
                    entityAttribute.Version,
                    kind.ToLower(),
                    pluralName);
            });
    }
}