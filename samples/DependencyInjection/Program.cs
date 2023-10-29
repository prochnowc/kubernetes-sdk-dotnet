// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using Kubernetes.Client;
using Kubernetes.Client.Extensions.DependencyInjection;
using Kubernetes.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using IHost host =
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(
            services =>
            {
                services.AddKubernetesClient()
                        .ConfigureFromKubeConfig();
            })
        .Build();

var client = host.Services.GetRequiredService<KubernetesClient>();

V1NamespaceList namespaces = await client.CoreV1().ListNamespaceAsync();
foreach (V1Namespace ns in namespaces.Items)
{
    Console.WriteLine(ns.Metadata?.Name);
}

KubernetesList<V1Namespace> list = await client.CustomObjects()
                                               .ListClusterCustomObjectAsync<V1Namespace>();

foreach (V1Namespace ns in list.Items)
{
    Console.WriteLine(ns.Metadata?.Name);
}
