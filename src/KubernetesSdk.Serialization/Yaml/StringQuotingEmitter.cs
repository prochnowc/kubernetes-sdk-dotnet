// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
 adapted from https://github.com/cloudbase/powershell-yaml/blob/master/powershell-yaml.psm1
*/

using System;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace Kubernetes.Serialization.Yaml;

/// <summary>
/// Provides a YAML string emitter.
/// </summary>
public sealed partial class StringQuotingEmitter : ChainedEventEmitter
{
    private const string QuotedRegexPattern =
        @"^(\~|null|Null|NULL|true|True|TRUE|false|False|FALSE|y|Y|yes|Yes|YES|on|On|ON|n|N|no|No|NO|off|Off|OFF|-?(0|[0-9]*)(\.[0-9]*)?([eE][-+]?[0-9]+)?)?$";

    /// <summary>
    /// Initializes a new instance of the <see cref="StringQuotingEmitter"/> class.
    /// </summary>
    /// <param name="next">The next <see cref="IEventEmitter"/>.</param>
    public StringQuotingEmitter(IEventEmitter next)
        : base(next)
    {
    }

    /// <inheritdoc/>
    public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
    {
        TypeCode typeCode = eventInfo.Source.Value != null
            ? Type.GetTypeCode(eventInfo.Source.Type)
            : TypeCode.Empty;

        switch (typeCode)
        {
            case TypeCode.Char:
                if (char.IsDigit((char)eventInfo.Source.Value!))
                {
                    eventInfo.Style = ScalarStyle.DoubleQuoted;
                }

                break;

            case TypeCode.String:
                string val = (string)eventInfo.Source.Value!;
                if (QuotedRegex().IsMatch(val))
                {
                    eventInfo.Style = ScalarStyle.DoubleQuoted;
                }
                else if (val.IndexOf('\n') > -1)
                {
                    eventInfo.Style = ScalarStyle.Literal;
                }

                break;
        }

        base.Emit(eventInfo, emitter);
    }
}