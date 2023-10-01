// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Fractions;

namespace Kubernetes.Models;

internal sealed class ResourceQuantityValue
{
    public string? Value { get; }

    public Fraction RationalValue { get; }

    public ResourceQuantityFormat Format { get; }

    public static ResourceQuantityValue Parse(string? value)
    {
        Fraction rationalValue;
        ResourceQuantityFormat format;

        if (string.IsNullOrWhiteSpace(value))
        {
            // No value has been defined, initialize to 0.
            rationalValue = new Fraction(0);
            format = ResourceQuantityFormat.BinarySI;
            return new ResourceQuantityValue(value, format, rationalValue);
        }

        value = value!.Trim();
        int si = value.IndexOfAny(ResourceQuantity.SuffixChars);
        if (si == -1)
        {
            si = value.Length;
        }

        Fraction literal = Fraction.FromString(value.Substring(0, si), CultureInfo.InvariantCulture);
        var suffixer = new Suffixer(value.Substring(si));

        rationalValue = literal.Multiply(Fraction.Pow(suffixer.Base, suffixer.Exponent));
        format = suffixer.Format;

        if (format == ResourceQuantityFormat.BinarySI && rationalValue > Fraction.FromDecimal(ResourceQuantity.MaxAllowed))
        {
            rationalValue = Fraction.FromDecimal(ResourceQuantity.MaxAllowed);
        }

        return new ResourceQuantityValue(value, format, rationalValue);
    }

    private static bool HasMantissa(Fraction value)
    {
        if (value.IsZero)
        {
            return false;
        }

        return BigInteger.Remainder(value.Numerator, value.Denominator) > 0;
    }

    internal ResourceQuantityValue(string? value, ResourceQuantityFormat format, Fraction rationalValue)
    {
        Value = value;
        RationalValue = rationalValue;
        Format = format;
    }

    // CanonicalizeString = go version CanonicalizeBytes
    // CanonicalizeBytes returns the canonical form of q and its suffix (see comment on Quantity).
    //
    // Note about BinarySI:
    // * If q.Format is set to BinarySI and q.Amount represents a non-zero value between
    //   -1 and +1, it will be emitted as if q.Format were DecimalSI.
    // * Otherwise, if q.Format is set to BinarySI, fractional parts of q.Amount will be
    //   rounded up. (1.1i becomes 2i.)
    public string CanonicalizeString(ResourceQuantityFormat format)
    {
        if (format == ResourceQuantityFormat.BinarySI)
        {
            if (RationalValue > -1024 && RationalValue < 1024)
            {
                return Suffixer.AppendMaxSuffix(RationalValue, ResourceQuantityFormat.DecimalSI);
            }

            if (HasMantissa(RationalValue))
            {
                return Suffixer.AppendMaxSuffix(RationalValue, ResourceQuantityFormat.DecimalSI);
            }
        }

        return Suffixer.AppendMaxSuffix(RationalValue, format);
    }

    public string CanonicalizeString()
    {
        return CanonicalizeString(Format);
    }

    private sealed class Suffixer
    {
        private static readonly IReadOnlyDictionary<string, (int, int)> BinSuffixes =
            new Dictionary<string, (int, int)>
            {
                // Don't emit an error when trying to produce
                // a suffix for 2^0.
                { string.Empty, (2, 0) },
                { "Ki", (2, 10) },
                { "Mi", (2, 20) },
                { "Gi", (2, 30) },
                { "Ti", (2, 40) },
                { "Pi", (2, 50) },
                { "Ei", (2, 60) },
            };

        private static readonly IReadOnlyDictionary<string, (int, int)> DecSuffixes =
            new Dictionary<string, (int, int)>
            {
                { "n", (10, -9) },
                { "u", (10, -6) },
                { "m", (10, -3) },
                { string.Empty, (10, 0) },
                { "k", (10, 3) },
                { "M", (10, 6) },
                { "G", (10, 9) },
                { "T", (10, 12) },
                { "P", (10, 15) },
                { "E", (10, 18) },
            };

        public Suffixer(string suffix)
        {
            // looked up
            {
                if (DecSuffixes.TryGetValue(suffix, out (int, int) be))
                {
                    (Base, Exponent) = be;
                    Format = ResourceQuantityFormat.DecimalSI;

                    return;
                }
            }

            {
                if (BinSuffixes.TryGetValue(suffix, out (int, int) be))
                {
                    (Base, Exponent) = be;
                    Format = ResourceQuantityFormat.BinarySI;

                    return;
                }
            }

            if (char.ToLower(suffix[0]) == 'e')
            {
                Base = 10;
                Exponent = int.Parse(suffix.Substring(1));
                Format = ResourceQuantityFormat.DecimalExponent;
                return;
            }

            throw new ArgumentException("unable to parse quantity's suffix");
        }

        public ResourceQuantityFormat Format { get; }

        public int Base { get; }

        public int Exponent { get; }

        public static string AppendMaxSuffix(Fraction value, ResourceQuantityFormat format)
        {
            if (value.IsZero)
            {
                return "0";
            }

            switch (format)
            {
                case ResourceQuantityFormat.DecimalExponent:
                {
                    int minE = -9;
                    Fraction lastv = Roundup(value * Fraction.Pow(10, -minE));

                    for (int exp = minE; ; exp += 3)
                    {
                        Fraction v = value * Fraction.Pow(10, -exp);
                        if (HasMantissa(v))
                        {
                            break;
                        }

                        minE = exp;
                        lastv = v;
                    }

                    if (minE == 0)
                    {
                        return $"{(decimal)lastv}";
                    }

                    return $"{(decimal)lastv}e{minE}";
                }

                case ResourceQuantityFormat.BinarySI:
                    return AppendMaxSuffix(value, BinSuffixes);
                case ResourceQuantityFormat.DecimalSI:
                    return AppendMaxSuffix(value, DecSuffixes);
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        private static string AppendMaxSuffix(Fraction value, IReadOnlyDictionary<string, (int, int)> suffixes)
        {
            KeyValuePair<string, (int, int)> min = suffixes.First();
            string? suffix = min.Key;
            Fraction lastv = Roundup(value * Fraction.Pow(min.Value.Item1, -min.Value.Item2));

            foreach (KeyValuePair<string, (int, int)> kv in suffixes.Skip(1))
            {
                Fraction v = value * Fraction.Pow(kv.Value.Item1, -kv.Value.Item2);
                if (HasMantissa(v))
                {
                    break;
                }

                suffix = kv.Key;
                lastv = v;
            }

            return $"{(decimal)lastv}{suffix}";
        }

        private static Fraction Roundup(Fraction lastv)
        {
            BigInteger round = BigInteger.DivRem(lastv.Numerator, lastv.Denominator, out BigInteger remainder);
            if (!remainder.IsZero)
            {
                lastv = round + 1;
            }

            return lastv;
        }
    }
}