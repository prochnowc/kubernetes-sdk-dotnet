// Copyright (c) Christian Prochnow and Contributors. All rights reserved.
// Licensed under the Apache-2.0 license. See LICENSE file in the project root for full license information.

/*
 NOTE: This file is derived from https://github.com/kubernetes-client/ licensed under the Apache-2.0 license.
*/

using System;

namespace Kubernetes.Models
{
    /// <summary>
    /// Describes object type in Kubernetes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class KubernetesEntityAttribute : Attribute
    {
        /// <summary>
        /// Gets the Group this Kubernetes type belongs to.
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// Gets the API Version of the Kubernetes type.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Gets the Kubernetes named schema this object is based on.
        /// </summary>
        public string Kind { get; }

        /// <summary>
        /// Gets or sets the plural name of the entity.
        /// </summary>
        public string? PluralName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KubernetesEntityAttribute"/> class.
        /// </summary>
        /// <param name="group">The group this Kubernetes type belongs to.</param>
        /// <param name="version">The API version of the Kubernetes type.</param>
        /// <param name="kind">The Kubernetes named schema this object is based on.</param>
        public KubernetesEntityAttribute(string group, string version, string kind)
        {
            Ensure.Arg.NotNull(group);
            Ensure.Arg.NotEmpty(version);
            Ensure.Arg.NotEmpty(kind);

            Group = group;
            Version = version;
            Kind = kind;
        }
    }
}
