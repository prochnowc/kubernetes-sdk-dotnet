// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Kubernetes.Models;

namespace Kubernetes.Client;

/// <summary>
/// Provides extension methods for <see cref="IKubernetesObject{TMetadata}"/>.
/// </summary>
public static class KubernetesObjectExtensions
{
    /// <summary>
    /// Tries to get the annotation with the specified key from the <see cref="IKubernetesObject{TMetadata}"/>.
    /// </summary>
    /// <param name="obj">The <see cref="IKubernetesObject{TMetadata}"/>.</param>
    /// <param name="key">The annotation key.</param>
    /// <param name="value">The annotation value.</param>
    /// <returns><c>true</c> if the annotation was found; <c>false</c> otherwise.</returns>
    public static bool TryGetAnnotation(
        this IKubernetesObject<V1ObjectMeta> obj,
        string key,
        [NotNullWhen(true)] out string? value)
    {
        Ensure.Arg.NotNull(obj);
        Ensure.Arg.NotNull(key);

        if (obj.Metadata.Annotations != null)
        {
            return obj.Metadata.Annotations.TryGetValue(key, out value);
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Gets the annotation with the specified key from the <see cref="IKubernetesObject{TMetadata}"/>.
    /// </summary>
    /// <param name="obj">The <see cref="IKubernetesObject{TMetadata}"/>.</param>
    /// <param name="key">The annotation key.</param>
    /// <param name="defaultValue">The default value to return if the annotation was not found.</param>
    /// <returns>The annotation value.</returns>
    public static string? GetAnnotation(
        this IKubernetesObject<V1ObjectMeta> obj,
        string key,
        string? defaultValue = default)
    {
        return TryGetAnnotation(obj, key, out string? value)
            ? value
            : defaultValue;
    }

    /// <summary>
    /// Sets the annotation with the specified key and value on the <see cref="IKubernetesObject{TMetadata}"/>.
    /// </summary>
    /// <param name="obj">The <see cref="IKubernetesObject{TMetadata}"/>.</param>
    /// <param name="key">The annotation key.</param>
    /// <param name="value">The annotation value. Specify <c>null</c> to remove the annotation.</param>
    public static void SetAnnotation(this IKubernetesObject<V1ObjectMeta> obj, string key, string? value)
    {
        Ensure.Arg.NotNull(obj);
        Ensure.Arg.NotNull(key);

        if (!string.IsNullOrEmpty(value))
        {
            obj.Metadata.Annotations ??= new Dictionary<string, string>();
            obj.Metadata.Annotations[key] = value;
        }
        else
        {
            obj.Metadata.Annotations?.Remove(key);
        }
    }

    /// <summary>
    /// Tries to get the label with the specified key from the <see cref="IKubernetesObject{TMetadata}"/>.
    /// </summary>
    /// <param name="obj">The <see cref="IKubernetesObject{TMetadata}"/>.</param>
    /// <param name="key">The label key.</param>
    /// <param name="value">The label value.</param>
    /// <returns><c>true</c> if the label was found; <c>false</c> otherwise.</returns>
    public static bool TryGetLabel(
        this IKubernetesObject<V1ObjectMeta> obj,
        string key,
        [NotNullWhen(true)] out string? value)
    {
        Ensure.Arg.NotNull(obj);
        Ensure.Arg.NotNull(key);

        if (obj.Metadata.Labels != null)
        {
            return obj.Metadata.Labels.TryGetValue(key, out value);
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Gets the label with the specified key from the <see cref="IKubernetesObject{TMetadata}"/>.
    /// </summary>
    /// <param name="obj">The <see cref="IKubernetesObject{TMetadata}"/>.</param>
    /// <param name="key">The label key.</param>
    /// <param name="defaultValue">The default value to return if the label was not found.</param>
    /// <returns>The label value.</returns>
    public static string? GetLabel(
        this IKubernetesObject<V1ObjectMeta> obj,
        string key,
        string? defaultValue = default)
    {
        return TryGetLabel(obj, key, out string? value)
            ? value
            : defaultValue;
    }

    /// <summary>
    /// Sets the label with the specified key and value on the <see cref="IKubernetesObject{TMetadata}"/>.
    /// </summary>
    /// <param name="obj">The <see cref="IKubernetesObject{TMetadata}"/>.</param>
    /// <param name="key">The label key.</param>
    /// <param name="value">The label value. Specify <c>null</c> to remove the label.</param>
    public static void SetLabel(this IKubernetesObject<V1ObjectMeta> obj, string key, string? value)
    {
        Ensure.Arg.NotNull(obj);
        Ensure.Arg.NotNull(key);

        if (!string.IsNullOrEmpty(value))
        {
            obj.Metadata.Labels ??= new Dictionary<string, string>();
            obj.Metadata.Labels[key] = value;
        }
        else
        {
            obj.Metadata.Labels?.Remove(key);
        }
    }
}