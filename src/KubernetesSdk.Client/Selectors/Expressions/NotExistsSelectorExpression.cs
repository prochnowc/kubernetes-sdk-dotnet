// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

namespace Kubernetes.Client.Selectors.Expressions;

/// <summary>
/// Represents a selector expression that matches resources by value absence.
/// </summary>
public sealed class NotExistsSelectorExpression : ISelectorExpression
{
    private readonly string _key;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotExistsSelectorExpression"/> class.
    /// </summary>
    /// <param name="key">The selector key.</param>
    public NotExistsSelectorExpression(string key)
    {
        Ensure.Arg.NotEmpty(key);

        _key = key;
    }

    /// <inheritdoc />
    public string GetExpression()
    {
        return "!" + _key;
    }
}