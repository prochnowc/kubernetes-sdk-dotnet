// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.IO;

namespace Kubernetes.Client;

internal interface IFileSystem
{
    Stream OpenRead(string path, bool async);

    bool FileExists(string path);
}