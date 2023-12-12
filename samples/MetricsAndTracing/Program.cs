// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using Kubernetes.Client;
using Kubernetes.Client.OpenTelemetry;
using Kubernetes.Models;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

using MeterProvider meterProvider = Sdk.CreateMeterProviderBuilder()
                                       .AddKubernetesClientInstrumentation()
                                       .AddHttpClientInstrumentation()
                                       .AddOtlpExporter(o => o.Protocol = OtlpExportProtocol.Grpc)
                                       .AddConsoleExporter()
                                       .Build() !;

using TracerProvider tracerProvider = Sdk.CreateTracerProviderBuilder()
                                       .AddKubernetesClientInstrumentation()
                                       .AddHttpClientInstrumentation()
                                       .AddOtlpExporter(o => o.Protocol = OtlpExportProtocol.Grpc)
                                       .AddConsoleExporter()
                                       .Build() !;

using var client = new KubernetesClient();

V1NamespaceList namespaces = await client.CoreV1().ListNamespaceAsync();
foreach (V1Namespace ns in namespaces.Items)
{
    Console.WriteLine(ns.Metadata.Name);
}

await client.CustomObjects().ListClusterCustomObjectAsync<V1Namespace>();