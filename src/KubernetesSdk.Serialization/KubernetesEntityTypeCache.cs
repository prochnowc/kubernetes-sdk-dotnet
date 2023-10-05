using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Kubernetes.Models;

namespace Kubernetes.Serialization;

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
                    throw new ArgumentException("No a kubernetes entity.");

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