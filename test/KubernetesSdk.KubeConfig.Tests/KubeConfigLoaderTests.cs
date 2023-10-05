// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace Kubernetes.KubeConfig;

public class KubeConfigLoaderTests
{
    [Fact]
    public async Task CanLoadKubeConfigFromPathAsync()
    {
        var loader = new KubeConfigLoader();
        await loader.LoadAsync("assets/kubeconfig.yml");
    }
}