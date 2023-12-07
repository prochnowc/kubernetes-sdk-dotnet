// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

namespace Kubernetes.Client.Mock;

public class TestFileSystem : IFileSystem
{
    private readonly Dictionary<string, TestFile> _files = new ();

    public TestFileSystem Add(TestFile file)
    {
        _files.Add(file.Path, file);
        return this;
    }

    public virtual Stream OpenRead(string path, bool async)
    {
        Ensure.Arg.NotEmpty(path);

        if (!_files.TryGetValue(path, out TestFile? file))
        {
            throw new FileNotFoundException();
        }

        return new MemoryStream(file.Content.ToArray());
    }

    public virtual bool FileExists(string path)
    {
        Ensure.Arg.NotEmpty(path);
        return _files.ContainsKey(path);
    }
}