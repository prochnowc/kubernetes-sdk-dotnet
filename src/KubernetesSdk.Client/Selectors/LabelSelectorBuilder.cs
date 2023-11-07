// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Kubernetes.Client.Selectors.Expressions;

namespace Kubernetes.Client.Selectors;

/// <summary>
/// Provides a builder for label selectors.
/// </summary>
public sealed class LabelSelectorBuilder
{
    /// <summary>
    /// Gets the expressions used to build the filter selector.
    /// </summary>
    public IList<ISelectorExpression> Expressions { get; } = new List<ISelectorExpression>();

    /// <summary>
    /// Initializes a new instance of the <see cref="LabelSelectorBuilder"/> class.
    /// </summary>
    internal LabelSelectorBuilder()
    {
    }

    /// <summary>
    /// Filters resources by label and value equality.
    /// </summary>
    /// <param name="label">The label name.</param>
    /// <param name="values">The label value.</param>
    /// <returns>The builder instance.</returns>
    public LabelSelectorBuilder Equals(string label, params string[] values)
    {
        Expressions.Add(new EqualsSelectorExpression(label, values));
        return this;
    }

    /// <summary>
    /// Filters resources by label and value in-equality.
    /// </summary>
    /// <param name="label">The label name.</param>
    /// <param name="values">The label value.</param>
    /// <returns>The builder instance.</returns>
    public LabelSelectorBuilder NotEquals(string label, params string[] values)
    {
        Expressions.Add(new NotEqualsSelectorExpression(label, values));
        return this;
    }

    /// <summary>
    /// Filters resources by label existence.
    /// </summary>
    /// <param name="label">The label name.</param>
    /// <returns>The builder instance.</returns>
    public LabelSelectorBuilder Exists(string label)
    {
        Expressions.Add(new ExistsSelectorExpression(label));
        return this;
    }

    /// <summary>
    /// Filters a resource by label absence.
    /// </summary>
    /// <param name="label">The label name.</param>
    /// <returns>The builder instance.</returns>
    public LabelSelectorBuilder NotExists(string label)
    {
        Expressions.Add(new NotExistsSelectorExpression(label));
        return this;
    }

    internal ISelectorExpression Build()
    {
        return new ChainedSelectorExpression(Expressions);
    }
}