// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kubernetes.Client;

internal static class FileSystem
{
    private const int DefaultBufferSize = 4096;

    private static readonly IFileSystem Default = new DefaultFileSystem();
    private static readonly AsyncLocal<IFileSystem?> Current = new ();

    private sealed class DefaultFileSystem : IFileSystem
    {
        public Stream OpenRead(string path, bool async)
        {
            if (async)
            {
                return new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    DefaultBufferSize,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);
            }

            return File.OpenRead(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }
    }

    [NotNull]
    public static IFileSystem? Instance
    {
        get => Current.Value ?? Default;
        set => Current.Value = value;
    }

    public static Stream OpenRead(string path, bool async = false)
    {
        return Instance.OpenRead(path, async);
    }

    public static bool FileExists(string path)
    {
        return Instance.FileExists(path);
    }

    public static string ReadAllText(string path)
    {
        using var reader = new StreamReader(OpenRead(path), Encoding.UTF8, true);
        return reader.ReadToEnd();
    }

    public static async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(OpenRead(path, true), Encoding.UTF8, true);
        return await reader.ReadToEndAsync();
    }

    public static byte[] ReadAllBytes(string path)
    {
        using Stream stream = OpenRead(path);

        byte[]? rentedArray = null;
        byte[] buffer = new byte[512];
        try
        {
            int bytesRead = 0;
            while (true)
            {
                if (bytesRead == buffer.Length)
                {
                    uint newLength = (uint)buffer.Length * 2;
                    if (newLength > int.MaxValue)
                    {
                        newLength = (uint)Math.Max(int.MaxValue, buffer.Length + 1);
                    }

                    byte[] tmp = ArrayPool<byte>.Shared.Rent((int)newLength);
                    Array.Copy(buffer, tmp, buffer.Length);
                    if (rentedArray != null)
                    {
                        ArrayPool<byte>.Shared.Return(rentedArray);
                    }

                    buffer = rentedArray = tmp;
                }

                int n = stream.Read(buffer, bytesRead, buffer.Length - bytesRead);
                if (n == 0)
                {
                    return buffer.AsSpan(0, bytesRead).ToArray();
                }

                bytesRead += n;
            }
        }
        finally
        {
            if (rentedArray != null)
            {
                ArrayPool<byte>.Shared.Return(rentedArray);
            }
        }
    }

    public static async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        using Stream stream = OpenRead(path, true);

        byte[] rentedArray = ArrayPool<byte>.Shared.Rent(512);
        try
        {
            int bytesRead = 0;
            while (true)
            {
                if (bytesRead == rentedArray.Length)
                {
                    uint newLength = (uint)rentedArray.Length * 2;
                    if (newLength > int.MaxValue)
                    {
                        newLength = (uint)Math.Max(int.MaxValue, rentedArray.Length + 1);
                    }

                    byte[] tmp = ArrayPool<byte>.Shared.Rent((int)newLength);
                    Buffer.BlockCopy(rentedArray, 0, tmp, 0, bytesRead);

                    byte[] toReturn = rentedArray;
                    rentedArray = tmp;

                    ArrayPool<byte>.Shared.Return(toReturn);
                }

                int n = await stream.ReadAsync(
                                        rentedArray,
                                        bytesRead,
                                        rentedArray.Length - bytesRead,
                                        cancellationToken)
                                    .ConfigureAwait(false);

                if (n == 0)
                {
                    return rentedArray.AsSpan(0, bytesRead).ToArray();
                }

                bytesRead += n;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentedArray);
        }
    }
}