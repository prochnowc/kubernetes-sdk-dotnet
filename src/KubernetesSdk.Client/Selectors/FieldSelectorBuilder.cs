// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Kubernetes.Client.Selectors.Expressions;

namespace Kubernetes.Client.Selectors;

/// <summary>
/// Provides a builder for field selectors.
/// </summary>
public sealed class FieldSelectorBuilder
{
    /// <summary>
    /// Gets the expressions used to build the field selector.
    /// </summary>
    public IList<ISelectorExpression> Expressions { get; } = new List<ISelectorExpression>();

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldSelectorBuilder"/> class.
    /// </summary>
    internal FieldSelectorBuilder()
    {
    }

    /// <summary>
    /// Filters resources by property name and value equality.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="value">The value of the property.</param>
    /// <returns>The builder instance.</returns>
    public FieldSelectorBuilder Equals(string propertyName, string value)
    {
        Expressions.Add(new EqualsSelectorExpression(propertyName, value));
        return this;
    }

    /// <summary>
    /// Filters resources by property name and value in-equality.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="value">The value of the property.</param>
    /// <returns>The builder instance.</returns>
    public FieldSelectorBuilder NotEquals(string propertyName, string value)
    {
        Expressions.Add(new NotEqualsSelectorExpression(propertyName, value));
        return this;
    }

    internal ISelectorExpression Build()
    {
        return new ChainedSelectorExpression(Expressions);
    }
}