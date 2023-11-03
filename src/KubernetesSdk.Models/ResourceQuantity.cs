// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System;
using System.Numerics;
using System.Text.Json.Serialization;
using Fractions;

namespace Kubernetes.Models;

public partial class ResourceQuantity
{
    /// <summary>
    /// Gets the maximum value.
    /// </summary>
    public static readonly decimal MaxValue = (decimal)BigInteger.Pow(2, 63) - 1;

    internal static readonly char[] SuffixChars = "eEinumkKMGTP".ToCharArray();

    private ResourceQuantityValue? _value;

    /// <summary>
    /// Gets the format of the resource quantity.
    /// </summary>
    [JsonIgnore]
    public ResourceQuantityFormat Format => GetValue().Format;

    private ResourceQuantity(decimal n, int exp, ResourceQuantityFormat format)
    {
        _value = new ResourceQuantityValue(null, format, Fraction.FromDecimal(n) * Fraction.Pow(10, exp));
    }

    private ResourceQuantityValue GetValue()
    {
        if (_value == null || !string.Equals(Value, _value.Value, StringComparison.InvariantCulture))
            _value = ResourceQuantityValue.Parse(Value);

        return _value;
    }

    public string ToString(ResourceQuantityFormat format)
    {
        return GetValue().CanonicalizeString(format);
    }

    public override string ToString()
    {
        return GetValue().CanonicalizeString();
    }

    public int ToInt32()
    {
        return GetValue().RationalValue.ToInt32();
    }

    public long ToInt64()
    {
        return GetValue().RationalValue.ToInt64();
    }

    public uint ToUInt32()
    {
        return GetValue().RationalValue.ToUInt32();
    }

    public ulong ToUInt64()
    {
        return GetValue().RationalValue.ToUInt64();
    }

    public BigInteger ToBigInteger()
    {
        return GetValue().RationalValue.ToBigInteger();
    }

    public decimal ToDecimal()
    {
        return GetValue().RationalValue.ToDecimal();
    }

    public double ToDouble()
    {
        return GetValue().RationalValue.ToDouble();
    }

    public static implicit operator decimal(ResourceQuantity? v)
    {
        return v?.ToDecimal() ?? 0;
    }

    public static implicit operator ResourceQuantity(decimal v)
    {
        return new ResourceQuantity(v, 0, ResourceQuantityFormat.DecimalExponent);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            ResourceQuantityValue value = GetValue();
            return ((int)value.Format * 397) ^ value.RationalValue.GetHashCode();
        }
    }

    protected bool Equals(ResourceQuantity? other)
    {
        ResourceQuantityValue value = GetValue();
        ResourceQuantityValue? otherValue = other?.GetValue();
        return value.Format == otherValue?.Format && value.RationalValue.Equals(otherValue.RationalValue);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        return Equals((ResourceQuantity)obj);
    }
}
