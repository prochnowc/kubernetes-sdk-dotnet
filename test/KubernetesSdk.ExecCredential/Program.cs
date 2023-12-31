﻿// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using Kubernetes.Models.KubeConfig;
using Kubernetes.Serialization;

if (args.Length > 0)
{
    if (args[0] == "wait")
    {
        Thread.Sleep(TimeSpan.FromSeconds(60));
        return 0;
    }

    if (args[0] == "empty")
    {
        return 0;
    }
}

string execInfo = Environment.GetEnvironmentVariable("KUBERNETES_EXEC_INFO") !;
IKubernetesSerializer serializer = KubernetesSerializerFactory.Instance.CreateSerializer("application/json");

ExecCredential? credentials;
try
{
    credentials = serializer.Deserialize<ExecCredential>(execInfo.AsSpan());
    if (credentials == null)
    {
        throw new ApplicationException("Received empty credential.");
    }
}
catch (Exception error)
{
    Console.WriteLine($"Failed to deserialize 'ExecCredential': {error.Message}");
    return 1;
}

var response = new ExecCredential(
    credentials.ApiVersion,
    status: new ExecCredentialStatus
    {
        ClientCertificateData = "Certificate",
        ClientKeyData = "CertificateKey",
        ExpirationTimestamp = DateTime.Now,
        Token = "Token",
    });

Console.WriteLine(serializer.Serialize(response));
return 0;