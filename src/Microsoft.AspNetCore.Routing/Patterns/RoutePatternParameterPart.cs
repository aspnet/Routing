// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.AspNetCore.Routing.Patterns
{
    [DebuggerDisplay("{DebuggerToString()}")]
    public class RoutePatternParameterPart : RoutePatternPart
    {
        internal RoutePatternParameterPart(
            string rawText,
            string name,
            object defaultValue,
            RoutePatternParameterKind parameterKind,
            RoutePatternConstraintReference[] constraints)
            : base(RoutePatternPartKind.Parameter, rawText)
        {
            // See #475 - this code should have some asserts, but it can't because of the design of RouteParameterParser.

            Name = name;
            DefaultValue = defaultValue;
            ParameterKind = parameterKind;
            Constraints = constraints;
        }

        public IReadOnlyList<RoutePatternConstraintReference> Constraints { get; }

        public object DefaultValue { get; }

        public bool IsCatchAll => ParameterKind == RoutePatternParameterKind.CatchAll;

        public bool IsOptional => ParameterKind == RoutePatternParameterKind.Optional;

        public RoutePatternParameterKind ParameterKind { get; }

        public string Name { get; }

        internal override string DebuggerToString()
        {
            if (RawText != null)
            {
                return RawText;
            }

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

            if (DefaultValue != null)
            {
                builder.Append("=");
                builder.Append(DefaultValue);
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
