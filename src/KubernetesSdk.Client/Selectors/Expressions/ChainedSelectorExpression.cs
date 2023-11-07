// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Kubernetes.Client.Selectors.Expressions;

internal sealed class ChainedSelectorExpression : ISelectorExpression
{
    private readonly IEnumerable<ISelectorExpression> _selectors;

    public ChainedSelectorExpression(IEnumerable<ISelectorExpression> selectors)
    {
        _selectors = selectors;
    }

    public string GetExpression()
    {
        return string.Join(",", _selectors.Select(s => s.GetExpression()));
    }
}