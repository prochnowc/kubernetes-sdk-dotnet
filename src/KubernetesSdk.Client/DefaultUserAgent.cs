// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Kubernetes.Client;

internal static class DefaultUserAgent
{
    private static readonly Lazy<ProductInfoHeaderValue> LazyValue = new (
        () =>
        {
            var versionAttribute =
                typeof(DefaultUserAgent).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            string version = versionAttribute.InformationalVersion;
            return new ProductInfoHeaderValue("Kubernetes_NET_SDK", version);
        });

    public static ProductInfoHeaderValue Value => LazyValue.Value;
}