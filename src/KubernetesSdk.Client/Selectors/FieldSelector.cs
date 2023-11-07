// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace Kubernetes.Client.Selectors;

/// <summary>
/// Provides a builder factory for field selectors.
/// </summary>
public static class FieldSelector
{
    /// <summary>
    /// Creates a field selector.
    /// </summary>
    /// <param name="configure">The delegate used to configure the builder.</param>
    /// <returns>The field selector.</returns>
    public static string Create(Action<FieldSelectorBuilder> configure)
    {
        Ensure.Arg.NotNull(configure);

        var builder = new FieldSelectorBuilder();
        configure(builder);
        return builder.Build()
                      .GetExpression();
    }
}