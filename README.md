# Kubernetes SDK for .NET

The Kubernetes SDK for .NET is a set of libraries and tools to develop software which
interacts with the Kubernetes open source container orchestrator.

The project is in a early stage but should already be usable.

Advantages over the official Kubernetes client for .NET:

* Nullable annotations
* Compatible with AOT
* Seamless integration with `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Http`
* Works with Windows containers

## The Kubernetes SDK for .NET

All libraries and tools are compatible with the following runtimes:

* netstandard >= 2.0
* .NET Framework >= 4.6.2
* .NET >= 6.0

The SDK is compatible with AOT compilation and uses static code generation for
JSON and YAML serialization.

## Quick start

### List namespaces in current cluster

```c#
using Kubernetes.Client;
using Kubernetes.Models;

var client = new KubernetesClient();
V1NamespaceList namespaces = await client.CoreV1().ListNamespaceAsync();
foreach (V1Namespace ns in namespaces.Items)
{
    Console.WriteLine(ns.Metadata?.Name);
}
```
