// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace Kubernetes.Serialization.Yaml;

/// <summary>
/// Provides a YAML float emitter.
/// </summary>
public sealed class FloatEmitter : ChainedEventEmitter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FloatEmitter"/> class.
    /// </summary>
    /// <param name="nextEmitter">The next emitter.</param>
    public FloatEmitter(IEventEmitter nextEmitter)
        : base(nextEmitter)
    {
    }

    /// <inheritdoc/>
    public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
    {
        switch (eventInfo.Source.Value)
        {
            // Floating point numbers should always render at least one zero (e.g. 1.0f => '1.0' not '1')
            case double d:
                emitter.Emit(new Scalar(d.ToString("0.0######################")));
                break;
            case float f:
                emitter.Emit(new Scalar(f.ToString("0.0######################")));
                break;
            default:
                base.Emit(eventInfo, emitter);
                break;
        }
    }
}