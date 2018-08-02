﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.AspNetCore.Routing.Patterns
{
    /// <summary>
    /// Represents a parameter part in a route pattern. Instances of <see cref="RoutePatternParameterPart"/>
    /// are immutable.
    /// </summary>
    [DebuggerDisplay("{DebuggerToString()}")]
    public sealed class RoutePatternParameterPart : RoutePatternPart
    {
        internal RoutePatternParameterPart(
            string parameterName,
            object @default,
            RoutePatternParameterKind parameterKind,
            RoutePatternConstraintReference[] constraints)
            : base(RoutePatternPartKind.Parameter)
        {
            // See #475 - this code should have some asserts, but it can't because of the design of RouteParameterParser.

            Name = parameterName;
            Default = @default;
            ParameterKind = parameterKind;
            Constraints = constraints;
        }

        /// <summary>
        /// Gets the list of constraints associated with this parameter.
        /// </summary>
        public IReadOnlyList<RoutePatternConstraintReference> Constraints { get; }

        /// <summary>
        /// Gets the default value of this route parameter. May be null.
        /// </summary>
        public object Default { get; }

        /// <summary>
        /// Returns <c>true</c> if this part is a catch-all parameter.
        /// Otherwise returns <c>false</c>.
        /// </summary>
        public bool IsCatchAll => ParameterKind == RoutePatternParameterKind.CatchAll;

        /// <summary>
        /// Returns <c>true</c> if this part is an optional parameter.
        /// Otherwise returns <c>false</c>.
        /// </summary>
        public bool IsOptional => ParameterKind == RoutePatternParameterKind.Optional;

        /// <summary>
        /// Gets the <see cref="RoutePatternParameterKind"/> of this parameter.
        /// </summary>
        public RoutePatternParameterKind ParameterKind { get; }

        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        public string Name { get; }

        internal override string DebuggerToString()
        {
            var builder = new StringBuilder();
            builder.Append("{");

            if (IsCatchAll)
            {
                builder.Append("*");
            }

            builder.Append(Name);

            foreach (var constraint in Constraints)
            {
                builder.Append(":");
                builder.Append(constraint.Constraint);
            }

            if (Default != null)
            {
                builder.Append("=");
                builder.Append(Default);
            }

            if (IsOptional)
            {
                builder.Append("?");
            }

            builder.Append("}");
            return builder.ToString();
        }
    }
}
