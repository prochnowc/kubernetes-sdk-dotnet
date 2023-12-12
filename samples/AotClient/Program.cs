// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using Kubernetes.Client;
using Kubernetes.Models;

try
{
    using var client = new KubernetesClient();
    VersionInfo code = await client.Version()
                                   .GetCodeAsync();

    Console.WriteLine(code.Major + "." + code.Minor);

    V1NamespaceList namespaces = await client.CoreV1().ListNamespaceAsync();
    foreach (V1Namespace ns in namespaces.Items)
    {
        Console.WriteLine(ns.Metadata.Name);
    }
}
catch (Exception error)
{
    Console.WriteLine(error.StackTrace);

    Exception? innerException = error.InnerException;
    while (innerException != null)
    {
        Console.WriteLine(innerException.Message);
        Console.WriteLine(innerException.StackTrace);

        innerException = innerException.InnerException;
    }
}
