// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;

namespace Kubernetes.Client;

public static class NSubstituteExtensions
{
    public static object Protected(this object substitute, string memberName, params object[] args)
    {
        MethodInfo method =
            substitute.GetType()
                      .GetMethod(memberName, BindingFlags.Instance | BindingFlags.NonPublic) !;

        if (!method.IsVirtual)
        {
            throw new Exception("Must be a virtual member");
        }

        return method.Invoke(substitute, args) !;
    }
}