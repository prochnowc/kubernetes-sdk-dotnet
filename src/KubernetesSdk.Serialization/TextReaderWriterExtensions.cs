using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Serialization;

#if !NET7_0_OR_GREATER

/// <summary>
/// API shims for <see cref="TextReader"/> and <see cref="TextWriter"/>.
/// </summary>
internal static class TextReaderWriterExtensions
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

    [SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "Not an async method")]
    private static Task WithCancellation(this Task task, CancellationToken cancellationToken)
    {
        return task.IsCompleted // fast-path optimization
            ? task
            : task.ContinueWith(
                completedTask => completedTask.GetAwaiter().GetResult(),
                cancellationToken,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
    }

    public static async Task<string> ReadToEndAsync(this TextReader reader, CancellationToken cancellationToken)
    {
        return await reader.ReadToEndAsync()
                           .WithCancellation(cancellationToken)
                           .ConfigureAwait(false);
    }

    public static async Task WriteAsync(this TextWriter writer, ReadOnlyMemory<char> buffer, CancellationToken cancellationToken)
    {
        await writer.WriteAsync(buffer.ToString())
                    .WithCancellation(cancellationToken)
                    .ConfigureAwait(false);
    }
}

#endif
