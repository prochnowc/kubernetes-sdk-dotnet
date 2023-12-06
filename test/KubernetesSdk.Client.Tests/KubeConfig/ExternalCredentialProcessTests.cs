using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using FluentAssertions;
using Kubernetes.Models.KubeConfig;

namespace Kubernetes.Client.KubeConfig;

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

    private const string CredentialProjectName = "KubernetesSdk.ExecCredential";

    private static string GetCommand()
    {
        return "dotnet";
    }

    private static List<string> GetArguments(params string[] args)
    {
        var result = new List<string>()
        {
            "run",
            "--project",
            @$"..\..\..\..\{CredentialProjectName}\{CredentialProjectName}.csproj",
            "-f",
            TargetFramework,
            "-c",
            Configuration,
            "--no-build",
        };

        result.AddRange(args);
        return result;
    }

    private void AssertExecCredentialsResponse(ExecCredential response)
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
                Arguments = GetArguments(),
            });

        ExecCredential response = await process.ExecuteAsync(TimeSpan.FromSeconds(10));
        AssertExecCredentialsResponse(response);
    }

    [Fact]
    public void ExecuteReturnsCredential()
    {
        var process = new ExternalCredentialProcess(
            new ExternalCredential
            {
                ApiVersion = "v1",
                Command = GetCommand(),
                Arguments = GetArguments(),
            });

        ExecCredential response = process.Execute(TimeSpan.FromSeconds(10));
        AssertExecCredentialsResponse(response);
    }

    [Fact]
    public async Task ExecuteAsyncThrowsTimeoutException()
    {
        var process = new ExternalCredentialProcess(
            new ExternalCredential
            {
                ApiVersion = "v1",
                Command = GetCommand(),
                Arguments = GetArguments("wait"),
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
                Arguments = GetArguments("wait"),
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
                Arguments = GetArguments("empty"),
            });

        await Assert.ThrowsAsync<KubernetesClientException>(
            async () => await process.ExecuteAsync(TimeSpan.FromSeconds(10)));
    }

    [Fact]
    public void ExecuteThrowsForEmptyResponse()
    {
        var process = new ExternalCredentialProcess(
            new ExternalCredential
            {
                ApiVersion = "v1",
                Command = GetCommand(),
                Arguments = GetArguments("empty"),
            });

        Assert.Throws<KubernetesClientException>(
            () => process.Execute(TimeSpan.FromSeconds(10)));
    }
}