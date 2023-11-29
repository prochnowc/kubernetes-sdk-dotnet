// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Kubernetes.Client;

/// <summary>
/// Builder for creating Kubernetes API request URIs.
/// </summary>
internal sealed partial class RequestUriBuilder
{
    private const string PathParametersRegexPattern = "\\{([^{}]+)\\}";

    private readonly string _pathTemplate;
    private readonly Dictionary<string, string> _parameters = new ();
    private readonly List<(string Name, string? Value)> _queryParameters = new ();

    public RequestUriBuilder(string pathTemplate)
    {
        _pathTemplate = pathTemplate;
    }

    public RequestUriBuilder AddPathParameter<T>(string name, T value)
    {
        _parameters.Add($"{{{name}}}", value?.ToString() !);
        return this;
    }

    public RequestUriBuilder AddQueryParameter<T>(string name, T value)
    {
        _queryParameters.Add((name, value?.ToString()));
        return this;
    }

    public override string ToString()
    {
        StringBuilder path = new (1024);

        path.Append(
            PathParametersRegex()
                .Replace(_pathTemplate, e => _parameters[e.Value]));

        int queryParameterCount = 0;
        for (int i = 0; i < _queryParameters.Count; i++)
        {
            if (_queryParameters[i].Value == null)
                continue;

            path.Append(queryParameterCount > 0 ? "&" : "?");
            path.Append(_queryParameters[i].Name);
            path.Append("=");
            path.Append(Uri.EscapeDataString(_queryParameters[i].Value!));
            ++queryParameterCount;
        }

        return path.ToString();
    }

    public Uri ToUri()
    {
        return new Uri(ToString(), UriKind.Relative);
    }
}
