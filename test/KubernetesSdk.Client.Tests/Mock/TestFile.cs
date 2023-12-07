// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Text;

namespace Kubernetes.Client.Mock;

public sealed class TestFile
{
    public string Path { get; }

    public ReadOnlyMemory<byte> Content { get; set; }

    public TestFile(string path, byte[] content)
    {
        Path = path;
        Content = content;
    }

    public TestFile(string path, string content)
    {
        Path = path;
        Content = Encoding.UTF8.GetBytes(content);
    }

    public void SetContent(string content)
    {
        Content = Encoding.UTF8.GetBytes(content);
    }
}