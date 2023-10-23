#if !NET7_0_OR_GREATER

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Client;

internal static class TextReaderExtensions
{
    [SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "Not an async method")]
    private static Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        return task.IsCompleted // fast-path optimization
            ? task
            : task.ContinueWith(
                completedTask => completedTask.GetAwaiter().GetResult(),
                cancellationToken,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
    }

    public static async Task<string?> ReadLineAsync(this TextReader reader, CancellationToken cancellationToken)
    {
        return await reader.ReadLineAsync()
                           .WithCancellation(cancellationToken)
                           .ConfigureAwait(false);
    }
}

#endif
