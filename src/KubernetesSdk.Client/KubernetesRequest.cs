// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Kubernetes.Client.Diagnostics;
using Kubernetes.Serialization;

namespace Kubernetes.Client;

/// <summary>
/// Represents a Kubernetes API request.
/// </summary>
[DebuggerDisplay("{DisplayName}")]
public sealed class KubernetesRequest
{
    private const string ContentType = "application/json";

    /// <summary>
    /// Gets the HTTP method of the request.
    /// </summary>
    public HttpMethod Method { get; }

    /// <summary>
    /// Gets the URI of the API endpoint.
    /// </summary>
    public Uri Uri { get; }

    /// <summary>
    /// Gets or sets the body of the request.
    /// </summary>
    public object? Content { get; set; }

    /// <summary>
    /// Gets or sets the timeout of the request.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Gets or sets the Kubernetes action of the request.
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Gets or sets the Kubernetes API version of the request.
    /// </summary>
    public string? ApiVersion { get; set; }

    /// <summary>
    /// Gets or sets the Kubernetes kind of the request.
    /// </summary>
    public string? Kind { get; set; }

    /// <summary>
    /// Gets the display name of the request.
    /// </summary>
    internal string DisplayName
    {
        get
        {
            if (!string.IsNullOrEmpty(ApiVersion) && !string.IsNullOrEmpty(Kind))
            {
                return $"{Action ?? Method.ToString()} {ApiVersion}.{Kind}";
            }

            string path = new Uri(new Uri("http://localhost", UriKind.Absolute), Uri).AbsolutePath;
            return $"{Action ?? Method.ToString()} {path}";
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KubernetesRequest"/> class.
    /// </summary>
    /// <param name="method">The HTTP method to use for the request.</param>
    /// <param name="uri">The URI of the API endpoint.</param>
    public KubernetesRequest(HttpMethod method, Uri uri)
    {
        Ensure.Arg.NotNull(method);
        Ensure.Arg.NotNull(uri);

        Method = method;
        Uri = uri;
    }

    internal TagList GetRequestTags()
    {
        return new TagList
        {
            new (OtelTags.KubernetesAction, Action),
            new (OtelTags.KubernetesApiVersion, ApiVersion),
            new (OtelTags.KubernetesKind, Kind),
        };
    }

    internal HttpRequestMessage CreateHttpRequest(
        KubernetesClientOptions options,
        IKubernetesSerializerFactory serializerFactory)
    {
        HttpContent? content = null;
        if (Content != null)
        {
            IKubernetesSerializer serializer = serializerFactory.CreateSerializer(ContentType);
            content = new StringContent(serializer.Serialize(Content));
        }

        var request = new HttpRequestMessage(Method, Uri)
        {
            Content = content,
        };

        // TODO: below code can probably go to KubernetesHttpClientFactory
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        request.Version = options.DisableHttp2
            ? HttpVersion.Version11
            : HttpVersion.Version20;
#endif

        request.Headers.UserAgent.Add(options.UserAgent ?? KubernetesClientDefaults.UserAgent);

        if (!string.IsNullOrWhiteSpace(options.TlsServerName))
        {
            request.Headers.Host = options.TlsServerName;
        }

        return request;
    }
}