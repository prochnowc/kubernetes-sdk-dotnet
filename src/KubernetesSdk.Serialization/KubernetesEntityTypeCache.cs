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

    private static readonly ConcurrentDictionary<string, KubernetesEntityType> CacheByName =
        new (StringComparer.OrdinalIgnoreCase);

    static KubernetesEntityTypeCache()
    {
        AppDomain.CurrentDomain.AssemblyLoad += (_, args) =>
        {
            if (!args.LoadedAssembly.IsDynamic)
            {
                AddAssemblyModelsToCache(args.LoadedAssembly);
            }
        };

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic))
        {
            AddAssemblyModelsToCache(assembly);
        }
    }

    private static string GetCacheKey(string group, string apiVersion, string kind)
    {
        return $"{group}_{apiVersion}_{kind}";
    }

    private static void AddAssemblyModelsToCache(Assembly assembly)
    {
        foreach (Type type in assembly.GetExportedTypes())
        {
            var entityAttribute = type.GetCustomAttribute<KubernetesEntityAttribute>();
            if (entityAttribute == null)
                continue;

            string kind = entityAttribute.Kind;
            string pluralName = string.IsNullOrWhiteSpace(entityAttribute.PluralName)
                ? $"{kind.ToLower()}s"
                : entityAttribute.PluralName!;

            var definition = new KubernetesEntityType(
                type,
                kind,
                $"{kind}List",
                entityAttribute.Group,
                entityAttribute.Version,
                kind.ToLower(),
                pluralName);

            CacheByType[type] = definition;
            CacheByName[GetCacheKey(entityAttribute.Group, entityAttribute.Version, entityAttribute.Kind)] = definition;
        }
    }

    public static KubernetesEntityType Get(Type type)
    {
        return CacheByType[type];
    }

    public static KubernetesEntityType Get(string group, string apiVersion, string kind)
    {
        return CacheByName[GetCacheKey(group, apiVersion, kind)];
    }
}