// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

namespace Kubernetes.Client.Selectors.Expressions;

/// <summary>
/// Represents a selector expression that matches resources by value in-equality.
/// </summary>
public sealed class NotEqualsSelectorExpression : ISelectorExpression
{
    private readonly string _key;
    private readonly string[] _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotEqualsSelectorExpression"/> class.
    /// </summary>
    /// <param name="key">The selector key.</param>
    /// <param name="values">The values to compare with.</param>
    public NotEqualsSelectorExpression(string key, params string[] values)
    {
        Ensure.Arg.NotEmpty(key);
        Ensure.Arg.NotNull(values);

        _key = key;
        _values = values;
    }

    /// <inheritdoc />
    public string GetExpression()
    {
        if (_values.Length == 1)
        {
            return $"{_key}!={_values[0]}";
        }

        return $"{_key} notin ({string.Join(",", _values)})";
    }
}