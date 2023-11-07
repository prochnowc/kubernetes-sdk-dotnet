// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

namespace Kubernetes.Client.Selectors.Expressions;

/// <summary>
/// Represents a selector expression.
/// </summary>
public interface ISelectorExpression
{
    /// <summary>
    /// Gets the selector expression.
    /// </summary>
    /// <returns>The selector expression.</returns>
    string GetExpression();
}