// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Kubernetes.Models.KubeConfig;
using Kubernetes.Serialization;

namespace Kubernetes.Client.KubeConfig;

/// <summary>
/// Represents the process to obtain external credentials.
/// </summary>
public sealed class ExternalCredentialProcess
{
    private readonly ExternalCredential _credential;
    private readonly IKubernetesSerializerFactory _serializerFactory;
    private readonly ProcessStartInfo _processStartInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalCredentialProcess"/> class.
    /// </summary>
    /// <param name="credential">The <see cref="ExternalCredential"/> providing information about the external credential process.</param>
    public ExternalCredentialProcess(ExternalCredential credential)
        : this(credential, KubernetesSerializerFactory.Instance)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalCredentialProcess"/> class.
    /// </summary>
    /// <param name="credential">The <see cref="ExternalCredential"/> providing information about the external credential process.</param>
    /// <param name="serializerFactory">The <see cref="IKubernetesSerializerFactory"/>.</param>
    public ExternalCredentialProcess(ExternalCredential credential, IKubernetesSerializerFactory serializerFactory)
    {
        Ensure.Arg.NotNull(credential);
        Ensure.Arg.NotNull(serializerFactory);

        _credential = credential;
        _serializerFactory = serializerFactory;
        _processStartInfo = CreateProcessStartInfo(credential);
    }

    private ProcessStartInfo CreateProcessStartInfo(ExternalCredential config)
    {
        var process = new ProcessStartInfo();

        var execInfo = new ExecCredential(
            config.ApiVersion,
            null,
            new ExecCredentialSpec
            {
                Interactive = Environment.UserInteractive,
            });

        IKubernetesSerializer serializer = _serializerFactory.CreateSerializer("application/json");

        process.EnvironmentVariables.Add("KUBERNETES_EXEC_INFO", serializer.Serialize(execInfo));

        foreach (IDictionary<string, string>? environmentVariable in config.EnvironmentVariables)
        {
            if (environmentVariable.TryGetValue("name", out string? environmentVariableName)
                && environmentVariable.TryGetValue("value", out string? environmentVariableValue))
            {
                process.EnvironmentVariables[environmentVariableName] = environmentVariableValue;
            }
        }

        process.FileName = config.Command;
        process.Arguments = string.Join(" ", config.Arguments);
        process.RedirectStandardOutput = true;
        process.RedirectStandardError = true;
        process.UseShellExecute = false;
        process.CreateNoWindow = true;

        return process;
    }

    /// <summary>
    /// Executes the external credential process and returns the provided credentials.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <returns>The <see cref="ExecCredential"/>.</returns>
    public ExecCredential Execute(TimeSpan timeout)
    {
        List<string> errors = new ();
        List<string> output = new ();

        void ErrorDataReceivedHandler(object sender, DataReceivedEventArgs args)
        {
            if (args.Data != null)
                errors.Add(args.Data);
        }

        void OutputDataReceivedHandler(object sender, DataReceivedEventArgs args)
        {
            if (args.Data != null)
                output.Add(args.Data);
        }

        using var process = new Process();
        process.StartInfo = _processStartInfo;
        process.EnableRaisingEvents = true;
        process.ErrorDataReceived += ErrorDataReceivedHandler;
        process.OutputDataReceived += OutputDataReceivedHandler;

        try
        {
            process.Start();
        }
        catch (Exception error)
        {
            throw new InvalidOperationException(
                $"Failed to start external process '{_processStartInfo.FileName}'",
                error);
        }

        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        if (!process.WaitForExit((int)timeout.TotalMilliseconds))
        {
            process.Kill();
            throw new TimeoutException($"External process '{_processStartInfo.FileName}' timed out");
        }

        return ProcessResponse(string.Concat(output));
    }

    /// <summary>
    /// Executes the external credential process and returns the provided credentials.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The <see cref="ExecCredential"/>.</returns>
    public async Task<ExecCredential> ExecuteAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        var tcs = new TaskCompletionSource<int>();

        void ExitedHandler(object? o, EventArgs args)
        {
            tcs.TrySetResult(((Process)o!).ExitCode);
        }

        using var process = new Process();
        process.StartInfo = _processStartInfo;
        process.EnableRaisingEvents = true;

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            process.Exited += ExitedHandler;

            try
            {
                process.Start();
            }
            catch (Exception error)
            {
                throw new InvalidOperationException(
                    $"Failed to start external process '{_processStartInfo.FileName}'",
                    error);
            }

            using CancellationTokenRegistration ctr = cts.Token.Register(
                p =>
                {
                    ((Process)p!).Kill();

                    if (cancellationToken.IsCancellationRequested)
                    {
                        tcs.TrySetException(new OperationCanceledException());
                    }
                    else
                    {
                        tcs.TrySetException(
                            new TimeoutException(
                                $"External process '{_processStartInfo.FileName}' timed out"));
                    }
                },
                process);

            Task<string> stderr = process.StandardError.ReadToEndAsync();
            Task<string> stdout = process.StandardOutput.ReadToEndAsync();

            await Task.WhenAll(stdout, stderr, tcs.Task)
                      .ConfigureAwait(false);

            string response = await stdout.ConfigureAwait(false);
            string errors = await stdout.ConfigureAwait(false);

            return ProcessResponse(response);
        }
        finally
        {
            process.Exited -= ExitedHandler;
        }
    }

    private ExecCredential ProcessResponse(string output)
    {
        IKubernetesSerializer serializer = _serializerFactory.CreateSerializer("application/json");

        ExecCredential? response;
        try
        {
            response = serializer.Deserialize<ExecCredential>(output.AsSpan());
            if (response == null)
                throw new ApplicationException("Received empty response");

            if (!string.Equals(response.ApiVersion, _credential.ApiVersion))
            {
                throw new NotSupportedException(
                    $"Received unsupported API version '{response.ApiVersion}', expected '{_credential.ApiVersion}'");
            }
        }
        catch (Exception error)
        {
            throw new AuthenticationException(
                $"Failed to receive credentials from external process '{_processStartInfo.FileName}'",
                error);
        }

        // TODO: do we need to check the exit code of the process?
        return response;
    }
}