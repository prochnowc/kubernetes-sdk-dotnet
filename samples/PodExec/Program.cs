// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Net.WebSockets;
using Kubernetes.Client;
using Kubernetes.Models;

using var client = new KubernetesClient();

V1PodList pods = await client.CoreV1().ListNamespacedPodAsync("dev");
foreach (V1Pod pod in pods.Items)
{
    Console.WriteLine(pod.Metadata.Name);
}

await using KubernetesWebSocket socket =
    await client.CoreV1()
                .ConnectNamespacedPodExecAsync(
                    "redis-master-0",
                    "dev",
                    new[] { "/bin/bash" });

await using Stream stdout = socket.GetOutputStream(1);
using var stdoutReader = new StreamReader(stdout);

await using Stream stderr = socket.GetOutputStream(2);
using var stderrReader = new StreamReader(stderr);

await using Stream stdin = socket.GetInputStream(0);
using var stdinWriter = new StreamWriter(stdin);

using CancellationTokenSource cancellationTokenSource = new ();

Console.CancelKeyPress += (sender, args) =>
{
    cancellationTokenSource.Cancel();
};

char[] stdoutBuffer = new char[1024];
char[] stderrBuffer = new char[1024];

Task<int> stdoutReaderTask = stdoutReader.ReadAsync(stdoutBuffer, 0, stdoutBuffer.Length);
Task<int> stderrReaderTask = stderrReader.ReadAsync(stderrBuffer, 0, stderrBuffer.Length);
Task stdinReaderTask = Task.Run(
    async () =>
    {
        while (!cancellationTokenSource.IsCancellationRequested)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            await stdinWriter.WriteAsync(key.KeyChar);
            await stdinWriter.FlushAsync();
        }
    });

while (!cancellationTokenSource.IsCancellationRequested)
{
    Task completedTask = await Task.WhenAny(new[] { stdoutReaderTask, stderrReaderTask });

    if (completedTask == stdoutReaderTask)
    {
        int charsRead = stdoutReaderTask.Result;
        Console.Out.Write(stdoutBuffer, 0, charsRead);
        stdoutReaderTask = stdoutReader.ReadAsync(stdoutBuffer, 0, stdoutBuffer.Length);
    }
    else if (completedTask == stderrReaderTask)
    {
        int charsRead = stderrReaderTask.Result;
        Console.Error.Write(stderrBuffer, 0, charsRead);
        stderrReaderTask = stderrReader.ReadAsync(stderrBuffer, 0, stderrBuffer.Length);
    }
}

_ = socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null);