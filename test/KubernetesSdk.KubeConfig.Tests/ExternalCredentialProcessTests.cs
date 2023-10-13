using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using FluentAssertions;
using Kubernetes.KubeConfig.Models;

namespace Kubernetes.KubeConfig;

public class ExternalCredentialProcessTests
{
    #if DEBUG
    private const string Configuration = "Debug";
    #else
    private const string Configuration = "Release";
    #endif

    #if NET6_0_OR_GREATER
    private const string TargetFramework = "net6.0";
    #else
    private const string TargetFramework = "net462";
    #endif

    private const string CredentialProjectName = "KubernetesSdk.KubeConfig.Credential";

    private static string GetCommand()
    {
        string command =
            @$"..\..\..\..\{CredentialProjectName}\bin\{Configuration}\{TargetFramework}\{CredentialProjectName}";

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            command += ".exe";

        return command;
    }

    private void AssetExecCredentialsResponse(ExecCredential response)
    {
        response.Status.Should()
                .NotBeNull();

        response.Status!.ClientCertificateData.Should()
                .Be("Certificate");

        response.Status!.ClientKeyData.Should()
                .Be("CertificateKey");

        response.Status!.Token.Should()
                .Be("Token");

        response.Status!.ExpirationTimestamp.Should()
                .BeWithin(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task ExecuteAsyncReturnsCredential()
    {
        var process = new ExternalCredentialProcess(
            new ExternalCredential
            {
                ApiVersion = "v1",
                Command = GetCommand(),
            });

        ExecCredential response = await process.ExecuteAsync(TimeSpan.FromSeconds(10));
        AssetExecCredentialsResponse(response);
    }

    [Fact]
    public void ExecuteReturnsCredential()
    {
        var process = new ExternalCredentialProcess(
            new ExternalCredential
            {
                ApiVersion = "v1",
                Command = GetCommand(),
            });

        ExecCredential response = process.Execute(TimeSpan.FromSeconds(10));
        AssetExecCredentialsResponse(response);
    }

    [Fact]
    public async Task ExecuteAsyncThrowsTimeoutException()
    {
        var process = new ExternalCredentialProcess(
            new ExternalCredential
            {
                ApiVersion = "v1",
                Command = GetCommand(),
                Arguments = { "wait" },
            });

        await Assert.ThrowsAsync<TimeoutException>(
            async () => await process.ExecuteAsync(TimeSpan.FromSeconds(10)));
    }

    [Fact]
    public void ExecuteThrowsTimeoutException()
    {
        var process = new ExternalCredentialProcess(
            new ExternalCredential
            {
                ApiVersion = "v1",
                Command = GetCommand(),
                Arguments = { "wait" },
            });

        Assert.Throws<TimeoutException>(
            () => process.Execute(TimeSpan.FromSeconds(10)));
    }

    [Fact]
    public async Task ExecuteAsyncThrowsForEmptyResponse()
    {
        var process = new ExternalCredentialProcess(
            new ExternalCredential
            {
                ApiVersion = "v1",
                Command = GetCommand(),
                Arguments = { "empty" },
            });

        await Assert.ThrowsAsync<AuthenticationException>(
            async () => await process.ExecuteAsync(TimeSpan.FromSeconds(10)));
    }
}