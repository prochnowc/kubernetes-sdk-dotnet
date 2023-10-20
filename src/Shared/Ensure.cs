// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Kubernetes;

internal static class Ensure
{
    public static class Arg
    {
        public static void NotNull(object? argument, [CallerArgumentExpression("argument")] string? paramName = null)
        {
            if (argument is null)
            {
                ThrowArgumentNullException(paramName);
            }
        }

        [DoesNotReturn]
        private static void ThrowArgumentNullException(string? paramName) =>
            throw new ArgumentNullException(paramName);

        public static void NotEmpty(string argument, [CallerArgumentExpression("argument")] string? paramName = null)
        {
            NotNull(argument, paramName);

            if (string.IsNullOrEmpty(argument))
            {
                ThrowArgumentException(paramName);
            }
        }

        [DoesNotReturn]
        private static void ThrowArgumentException(string? paramName) =>
            throw new ArgumentException($"Argument should not be empty", paramName);
    }
}