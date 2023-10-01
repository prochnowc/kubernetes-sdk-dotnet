using System.Diagnostics;
using System.Reflection;
using Kubernetes.Client.Extensions.DependencyInjection;
using Kubernetes.Client.KubeConfig;
using Kubernetes.KubeConfig.Models;
using Kubernetes.Models;
using Kubernetes.Serialization;
using Kubernetes.Serialization.Json;
using Kubernetes.Serialization.Yaml;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Kubernetes.Client;

public static class NSubstituteExtensions
{
    public static object Protected(this object substitute, string memberName, params object[] args)
    {
        MethodInfo method =
            substitute.GetType()
                      .GetMethod(memberName, BindingFlags.Instance | BindingFlags.NonPublic)!;

        if (!method.IsVirtual)
        {
            throw new Exception("Must be a virtual member");
        }

        return method.Invoke(substitute, args)!;
    }
}

public class KubeConfigOptionsProviderTests
{
    [Fact]
    public void OptionsArePopulatedFromConfig()
    {
        V1Config config = new();

        var serializer = Substitute.For<IKubernetesSerializerFactory>();
        var provider = Substitute.ForPartsOf<KubeConfigOptionsProvider>(serializer);
        provider.Protected("LoadConfig", Arg.Any<string>())
                .Returns(config);

        KubernetesClientOptions options = provider.CreateOptions();
    }

    [Fact]
    public async Task LoadTest()
    {
        var serializer =
            new KubernetesSerializerFactory(
                new IKubernetesSerializer[]
                {
                    new KubernetesJsonSerializer(),
                    new KubernetesYamlSerializer()
                });

        var options = new KubeConfigOptionsProvider(
            Enumerable.Empty<IAuthProviderOptionsBinder>(),
            serializer)
        {
            Context = "afp-dev-cluster"
        };

        var client = new KubernetesClient(options.CreateOptions(), serializer);

        V1Deployment deployment = await client.AppsV1()
                                              .ReadNamespacedDeploymentAsync("coredns", "kube-system");

        V1PodList result = await client.CoreV1()
                                       .ListPodForAllNamespacesAsync();

        V1Pod pod = result.Items.First();
        await using Stream stream = await client.CoreV1()
                                    .ReadNamespacedPodLogAsync(pod.Metadata.Name + "a", pod.Metadata.Namespace);

        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream)
        {
            string? line = await reader.ReadLineAsync();
            Debug.WriteLine(line);
        }
    }

    [Fact]
    public async Task TestBuilder()
    {
        var services = new ServiceCollection();
        services.AddKubernetesClient();

        ServiceProvider sp = services.BuildServiceProvider();
        var client = sp.GetRequiredService<KubernetesClient>();

        var client2 = sp.GetRequiredService<KubernetesClient>();
    }

    [Fact]
    public async Task WatchTest()
    {
        var serializer =
            new KubernetesSerializerFactory(
                new IKubernetesSerializer[]
                {
                    new KubernetesJsonSerializer(),
                    new KubernetesYamlSerializer()
                });

        var options = new KubeConfigOptionsProvider(
            Enumerable.Empty<IAuthProviderOptionsBinder>(),
            serializer)
        {
            Context = "afp-dev-cluster"
        };

        var client = new KubernetesClient(options.CreateOptions(), serializer);

        using IWatcher<V1Pod> watcher = await client.CoreV1()
                                              .WatchPodForAllNamespacesAsync();

        WatchEvent<V1Pod>? @event = await watcher.ReadNextAsync(CancellationToken.None);
        while (@event != null)
        {
            @event = await watcher.ReadNextAsync(CancellationToken.None);
        }
    }
}