// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kubernetes.Client.Mock;

namespace Kubernetes.Client.Authentication;

public class ServiceAccountTokenProviderTests
{
    [Fact]
    public async Task ReadsTokenFromFile()
    {
        var tokenProvider = new ServiceAccountTokenProvider("assets/service-account-token");
        string token = await tokenProvider.GetTokenAsync(false, CancellationToken.None);
        token.Should()
             .Be("some-token");
    }

    [Fact]
    public async Task RefreshesTokenFromFile()
    {
        var tokenFile = new TestFile("assets/service-account-token", "some-token");

        TestFileSystem fileSystem = new TestFileSystem()
            .Add(tokenFile);

        FileSystem.Instance = fileSystem;

        var timeProvider = new TestTimeProvider();
        TimeProvider.Instance = timeProvider;

        var tokenProvider = new ServiceAccountTokenProvider("assets/service-account-token");
        _ = await tokenProvider.GetTokenAsync(false, CancellationToken.None);

        timeProvider.UtcNow += ServiceAccountTokenProvider.TokenLifetime;
        tokenFile.SetContent("some-refreshed-token");

        string token = await tokenProvider.GetTokenAsync(false, CancellationToken.None);
        token.Should()
             .Be("some-refreshed-token");
    }
}