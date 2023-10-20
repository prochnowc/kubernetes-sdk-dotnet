// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Kubernetes.Serialization;

internal sealed class ValueDefaultsInitializer<T>
    where T : new()
{
    private readonly List<Action<T>> _configurations = new ();
    private T? _value;
    private bool _dirty;

    public T Value
    {
        get
        {
            if (_value == null || _dirty)
            {
                lock (this)
                {
                    if (_value == null || _dirty)
                    {
                        _value = new T();
                        PopulateValue(_value);
                    }

                    _dirty = false;
                }
            }

            return _value;
        }
    }

    public void Configure(Action<T> configure)
    {
        Ensure.Arg.NotNull(configure);
        lock (this)
        {
            _configurations.Add(configure);
            _dirty = true;
        }
    }

    public void PopulateValue(T value)
    {
        lock (this)
        {
            foreach (Action<T> configuration in _configurations)
            {
                configuration(value);
            }
        }
    }
}