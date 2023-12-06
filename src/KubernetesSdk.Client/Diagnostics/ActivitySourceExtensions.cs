// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Kubernetes.Client.Diagnostics;

/// <summary>
/// Provides tracing for the Kubernetes API client.
/// </summary>
public static class ActivitySourceExtensions
{
    internal static Activity? StartActivity(
        this ActivitySource activitySource,
        Type sourceType,
        string name,
        ActivityKind kind,
        Func<TagList>? tagsFactory = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        if (!activitySource.HasListeners())
            return null;

        TagList tags = tagsFactory?.Invoke() ?? new ();
        tags.Add(OtelTags.CodeNamespace, sourceType.FullName);
        tags.Add(OtelTags.CodeFunction, memberName);
        tags.Add(OtelTags.CodeFilePath, filePath);
        tags.Add(OtelTags.CodeLineNo, lineNumber);

        return activitySource.StartActivity(name, kind, default(ActivityContext), tags);
    }

    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument", Justification = "Passed by caller.")]
    internal static Activity? StartActivity(
        this ActivitySource activitySource,
        object source,
        string name,
        ActivityKind kind,
        Func<TagList>? tagsFactory = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        return StartActivity(activitySource, source.GetType(), name, kind, tagsFactory, memberName, filePath, lineNumber);
    }
}