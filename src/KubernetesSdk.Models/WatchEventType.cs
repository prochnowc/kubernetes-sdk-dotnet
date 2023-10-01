// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

namespace Kubernetes.Models;

/// <summary>
/// Describes the type of a watch event.
/// </summary>
public enum WatchEventType
{
    /// <summary>
    /// Emitted when an object is created, modified to match a watch's filter, or when a watch is first opened.
    /// </summary>
    Added,

    /// <summary>
    /// Emitted when an object is modified.
    /// </summary>
    Modified,

    /// <summary>
    /// Emitted when an object is deleted or modified to no longer match a watch's filter.
    /// </summary>
    Deleted,

    /// <summary>
    /// Emitted when an error occurs while watching resources. Most commonly, the error is 410 Gone which indicates that
    /// the watch resource version was outdated and events were probably lost. In that case, the watch should be restarted.
    /// </summary>
    Error,

    /// <summary>
    /// Bookmarks may be emitted periodically to update the resource version. The object will
    /// contain only the resource version.
    /// </summary>
    Bookmark,
}