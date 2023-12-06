# Kubernetes SDK for .NET

![Nuget](https://img.shields.io/nuget/v/KubernetesSdk.Client?label=NuGet) ![MyGet](https://img.shields.io/myget/kubernetes-sdk-dotnet/vpre/KubernetesSdk.Client?label=MyGet)

The Kubernetes SDK for .NET is a set of libraries and tools to develop software which
interacts with the Kubernetes open source container orchestrator.

The project is in a early stage but should already be quiet usable.

Advantages over the official Kubernetes client for .NET:

* Nullable annotations
* Compatible with AOT
* Seamless integration with `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Http`
* Supports .NET Tracing and Metrics including OpenTelemetry
* Works with Windows containers

## The Kubernetes SDK for .NET

All libraries and tools are compatible with the following runtimes:

* netstandard >= 2.0
* .NET Framework >= 4.6.2
* .NET >= 6.0

The SDK is compatible with AOT compilation and uses static code generation for
JSON and YAML serialization.

## Quick start

Install the client NuGet package:

```shell
dotnet add package KubernetesSdk.Client
```

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

### Configuring the client

By default the `KubernetesClient` constructor will resolve it's configuration from `~/.kube/config`, or, when
running in a cluster, from the in-cluster config.

You can explicitly pass an instance of `KubernetesClientOptions`
to the constructor, by either manually configuring an instance of `KubernetesClientOptions`, or by using one
of the pre-defined option providers `DefaultOptionsProvider`, `KubeConfigOptionsProvider` or `InClusterOptionsProvider`:

```csharp
var optionsProvider = new KubeConfigOptionsProvider() { Context = "my-context" };
KubernetesCLientOptions options = optionsProvider.CreateOptions();
var client = new KubernetesClient(options);
```

The code above will instantiate a new `KubernetesClient` using the options resolved from the cluster context `my-context`
defined in the `~/.kube/config` file.

## Using the Microsoft.Extensions integration

If you want to use the integration with `Microsoft.Extensions.DependencyInjection` and
`Microsoft.Extensions.Http` you need to install the following package:

```shell
dotnet add package KubernetesSdk.Client.Extensions.DependencyInjection
```

Registering the client with the DI container is done using the `AddKubernetesClient()`
extension method:

```csharp
using Kubernetes.Client.Extensions.DependencyInjection;

using IHost host =
    Host.CreateDefaultBuilder(args)
        .ConfigureServices(
            services =>
            {
                services.AddKubernetesClient();
            })
        .Build();
```

The above code will register the `KubernetesClient` using default configuration. To use non-default
configuration you can call one of the various `ConfigureXX()` methods on the client builder:

```csharp
services.AddKubernetesClient()
        .ConfigureFromKubeConfig();
```

### Registering named clients

For advanced scenarios it is also possible to register multiple clients with different configuration.
To register a named client simply pass a name to the `AddKubernetesClient()` extension method:

```csharp
services.AddKubernetesClient("Client1");
```

Named clients can be resolved using the `IKubernetesClientFactory`:

```csharp
var clientFactory = serviceProvider.GetRequiredService<IKubernetesClientFactory>();
KubernetesClient client = clientFactory.CreateClient("Client1");
```

Note that the name for the default client is `string.Empty` and can always be resolved
directly, without using the factory.
