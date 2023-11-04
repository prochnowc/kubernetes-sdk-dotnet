// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System;
using System.Numerics;
using System.Text.Json.Serialization;
using Fractions;
using YamlDotNet.Serialization;

namespace Kubernetes.Models;

/// <content>
/// Provides methods to convert between <see cref="ResourceQuantityValue"/> and <see cref="ResourceQuantity"/>.
/// </content>
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
    [YamlIgnore]
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

    /// <summary>
    /// Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <param name="format">Specifies the format of the formatted value.</param>
    /// <returns>The <see cref="ResourceQuantity"/> formatted as a string.</returns>
    public string ToString(ResourceQuantityFormat format)
    {
        return GetValue().CanonicalizeString(format);
    }

    /// <summary>
    /// Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <remarks>
    /// The string is formatted using the <see cref="ResourceQuantityFormat"/> of the <see cref="ResourceQuantity"/>.
    /// </remarks>
    /// <returns>The <see cref="ResourceQuantity"/> formatted as a string.</returns>
    public override string ToString()
    {
        return GetValue().CanonicalizeString();
    }

    /// <summary>
    /// Converts the value of this instance to an <see cref="int"/>.
    /// </summary>
    /// <returns>The value of this instance converted to an <see cref="int"/>.</returns>
    public int ToInt32()
    {
        return GetValue().RationalValue.ToInt32();
    }

    /// <summary>
    /// Converts the value of this instance to an <see cref="long"/>.
    /// </summary>
    /// <returns>The value of this instance converted to an <see cref="long"/>.</returns>
    public long ToInt64()
    {
        return GetValue().RationalValue.ToInt64();
    }

    /// <summary>
    /// Converts the value of this instance to an <see cref="uint"/>.
    /// </summary>
    /// <returns>The value of this instance converted to an <see cref="uint"/>.</returns>
    public uint ToUInt32()
    {
        return GetValue().RationalValue.ToUInt32();
    }

    /// <summary>
    /// Converts the value of this instance to an <see cref="ulong"/>.
    /// </summary>
    /// <returns>The value of this instance converted to an <see cref="ulong"/>.</returns>
    public ulong ToUInt64()
    {
        return GetValue().RationalValue.ToUInt64();
    }

    /// <summary>
    /// Converts the value of this instance to an <see cref="BigInteger"/>.
    /// </summary>
    /// <returns>The value of this instance converted to an <see cref="BigInteger"/>.</returns>
    public BigInteger ToBigInteger()
    {
        return GetValue().RationalValue.ToBigInteger();
    }

    /// <summary>
    /// Converts the value of this instance to a <see cref="decimal"/>.
    /// </summary>
    /// <returns>The value of this instance converted to a <see cref="decimal"/>.</returns>
    public decimal ToDecimal()
    {
        return GetValue().RationalValue.ToDecimal();
    }

    /// <summary>
    /// Converts the value of this instance to a <see cref="double"/>.
    /// </summary>
    /// <returns>The value of this instance converted to a <see cref="double"/>.</returns>
    public double ToDouble()
    {
        return GetValue().RationalValue.ToDouble();
    }

    /// <summary>
    /// Implicit conversion from <see cref="ResourceQuantity"/> to <see cref="decimal"/>.
    /// </summary>
    /// <param name="v">The <see cref="ResourceQuantity"/> to convert.</param>
    /// <returns>The converted <see cref="decimal"/> value.</returns>
    public static implicit operator decimal(ResourceQuantity? v)
    {
        return v?.ToDecimal() ?? 0;
    }

    /// <summary>
    /// Implicit conversion from <see cref="decimal"/> to <see cref="ResourceQuantity"/> using
    /// <see cref="ResourceQuantityFormat.DecimalExponent"/>.
    /// </summary>
    /// <param name="v">The <see cref="decimal"/> to convert.</param>
    /// <returns>The converted <see cref="ResourceQuantity"/> value.</returns>
    public static implicit operator ResourceQuantity(decimal v)
    {
        return new ResourceQuantity(v, 0, ResourceQuantityFormat.DecimalExponent);
    }

    /// <summary>
    /// Gets the hash code for this instance.
    /// </summary>
    /// <returns>The hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            ResourceQuantityValue value = GetValue();
            return ((int)value.Format * 397) ^ value.RationalValue.GetHashCode();
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="ResourceQuantity"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="ResourceQuantity"/> to compare with this instance.</param>
    /// <returns><c>true</c> if the specified <see cref="ResourceQuantity"/> is equal to this instance; otherwise, <c>false</c>.</returns>
    protected bool Equals(ResourceQuantity? other)
    {
        ResourceQuantityValue value = GetValue();
        ResourceQuantityValue? otherValue = other?.GetValue();
        return value.Format == otherValue?.Format && value.RationalValue.Equals(otherValue.RationalValue);
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
    /// <returns><c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.</returns>
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
