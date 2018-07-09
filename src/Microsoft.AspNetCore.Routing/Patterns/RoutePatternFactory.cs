// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing.Constraints;

namespace Microsoft.AspNetCore.Routing.Patterns
{
    public static class RoutePatternFactory
    {
        public static RoutePattern Pattern(IEnumerable<RoutePatternPathSegment> segments)
        {
            if (segments == null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            return PatternCore(null, null, null, segments);
        }

        public static RoutePattern Pattern(string text, IEnumerable<RoutePatternPathSegment> segments)
        {
            if (segments == null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            return PatternCore(text, null, null, segments);
        }

        public static RoutePattern Pattern(
            object defaults,
            object constraints,
            IEnumerable<RoutePatternPathSegment> segments)
        {
            if (segments == null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            return PatternCore(null, new RouteValueDictionary(defaults), new RouteValueDictionary(constraints), segments);
        }

        public static RoutePattern Pattern(
            string text,
            object defaults,
            object constraints,
            IEnumerable<RoutePatternPathSegment> segments)
        {
            if (segments == null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            return PatternCore(text, new RouteValueDictionary(defaults), new RouteValueDictionary(constraints), segments);
        }

        public static RoutePattern Pattern(params RoutePatternPathSegment[] segments)
        {
            if (segments == null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            return PatternCore(null, null, null, segments);
        }

        public static RoutePattern Pattern(string text, params RoutePatternPathSegment[] segments)
        {
            if (segments == null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            return PatternCore(text, null, null, segments);
        }

        public static RoutePattern Pattern(
            object defaults,
            object constraints,
            params RoutePatternPathSegment[] segments)
        {
            if (segments == null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            return PatternCore(null, new RouteValueDictionary(defaults), new RouteValueDictionary(constraints), segments);
        }

        public static RoutePattern Pattern(
            string text,
            object defaults,
            object constraints,
            params RoutePatternPathSegment[] segments)
        {
            if (segments == null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            return PatternCore(text, new RouteValueDictionary(defaults), new RouteValueDictionary(constraints), segments);
        }

        private static RoutePattern PatternCore(
            string text,
            IDictionary<string, object> defaults,
            IDictionary<string, object> constraints,
            IEnumerable<RoutePatternPathSegment> segments)
        {
            // We want to merge the segment data with the 'out of line' defaults and constraints.
            //
            // This means that for parameters that have 'out of line' defaults we will modify
            // the parameter to contain the default (same story for constraints).
            //
            // We also maintain a collection of defaults and constraints that will also
            // contain the values that don't match a parameter.
            //
            // It's important that these two views of the data are consistent. We don't want
            // values specified out of line to have a different behavior.

            var updatedDefaults = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (defaults != null)
            {
                foreach (var kvp in defaults)
                {
                    updatedDefaults.Add(kvp.Key, kvp.Value);
                }
            }

            var updatedConstraints = new Dictionary<string, List<RoutePatternConstraintReference>>(StringComparer.OrdinalIgnoreCase);
            if (constraints != null)
            {
                foreach (var kvp in constraints)
                {
                    updatedConstraints.Add(kvp.Key, new List<RoutePatternConstraintReference>()
                {
                    Constraint(kvp.Key, kvp.Value),
                });
                }
            }

            var updatedSegments = segments.ToArray();
            for (var i = 0; i < updatedSegments.Length; i++)
            {
                updatedSegments[i] = VisitSegment(updatedSegments[i]);
            }

            var parameters = new List<RoutePatternParameterPart>();
            for (var i = 0; i < updatedSegments.Length; i++)
            {
                var segment = updatedSegments[i];
                for (var j = 0; j < segment.Parts.Count; j++)
                {
                    if (segment.Parts[j] is RoutePatternParameterPart parameter)
                    {
                        parameters.Add(parameter);
                    }
                }
            }

            return new RoutePattern(
                text,
                updatedDefaults,
                updatedConstraints.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<RoutePatternConstraintReference>)kvp.Value.ToArray()),
                parameters.ToArray(),
                updatedSegments.ToArray());

            RoutePatternPathSegment VisitSegment(RoutePatternPathSegment segment)
            {
                var updatedParts = new RoutePatternPart[segment.Parts.Count];
                for (var i = 0; i < segment.Parts.Count; i++)
                {
                    var part = segment.Parts[i];
                    updatedParts[i] = VisitPart(part);
                }

                return SegmentCore(segment.RawText, updatedParts);
            }

            RoutePatternPart VisitPart(RoutePatternPart part)
            {
                if (!part.IsParameter)
                {
                    return part;
                }

                var parameter = (RoutePatternParameterPart)part;
                var @default = parameter.DefaultValue;

                if (updatedDefaults.TryGetValue(parameter.Name, out var newDefault))
                {
                    if (parameter.DefaultValue != null)
                    {
                        var message = Resources.FormatTemplateRoute_CannotHaveDefaultValueSpecifiedInlineAndExplicitly(parameter.Name);
                        throw new InvalidOperationException(message);
                    }

                    if (parameter.IsOptional)
                    {
                        var message = Resources.TemplateRoute_OptionalCannotHaveDefaultValue;
                        throw new InvalidOperationException(message);
                    }

                    @default = newDefault;
                }
                
                if (parameter.DefaultValue != null)
                {
                    updatedDefaults.Add(parameter.Name, parameter.DefaultValue);
                }

                if (!updatedConstraints.TryGetValue(parameter.Name, out var parameterConstraints) &&
                    parameter.Constraints.Count > 0)
                {
                    parameterConstraints = new List<RoutePatternConstraintReference>();
                    updatedConstraints.Add(parameter.Name, parameterConstraints);
                }

                if (parameter.Constraints.Count > 0)
                {
                    parameterConstraints.AddRange(parameter.Constraints);
                }

                return ParameterPartCore(
                    parameter.RawText,
                    parameter.Name,
                    @default,
                    parameter.ParameterKind,
                    (IEnumerable<RoutePatternConstraintReference>)parameterConstraints ?? Array.Empty<RoutePatternConstraintReference>());
            }
        }

        public static RoutePatternPathSegment Segment(IEnumerable<RoutePatternPart> parts)
        {
            if (parts == null)
            {
                throw new ArgumentNullException(nameof(parts));
            }

            return SegmentCore(null, parts);
        }

        public static RoutePatternPathSegment Segment(string text, IEnumerable<RoutePatternPart> parts)
        {
            if (parts == null)
            {
                throw new ArgumentNullException(nameof(parts));
            }

            return SegmentCore(text, parts);
        }

        public static RoutePatternPathSegment Segment(params RoutePatternPart[] parts)
        {
            if (parts == null)
            {
                throw new ArgumentNullException(nameof(parts));
            }

            return SegmentCore(null, parts);
        }

        public static RoutePatternPathSegment Segment(string text, params RoutePatternPart[] parts)
        {
            if (parts == null)
            {
                throw new ArgumentNullException(nameof(parts));
            }

            return SegmentCore(text, parts);
        }

        private static RoutePatternPathSegment SegmentCore(
            string text,
            IEnumerable<RoutePatternPart> parts)
        {
            return new RoutePatternPathSegment(text, parts.ToArray());
        }

        public static RoutePatternLiteralPart LiteralPart(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(content));
            }

            if (content.IndexOf('?') >= 0)
            {
                throw new ArgumentException(Resources.FormatTemplateRoute_InvalidLiteral(content));
            }

            return LiteralPartCore(null, content);
        }

        public static RoutePatternLiteralPart LiteralPart(string text, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(content));
            }

            if (content.IndexOf('?') >= 0)
            {
                throw new ArgumentException(Resources.FormatTemplateRoute_InvalidLiteral(content));
            }

            return LiteralPartCore(text, content);
        }

        private static RoutePatternLiteralPart LiteralPartCore(string text, string content)
        {
            return new RoutePatternLiteralPart(text, content);
        }

        public static RoutePatternSeparatorPart SeparatorPart(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(content));
            }

            return SeparatorPartCore(null, content);
        }

        public static RoutePatternSeparatorPart SeparatorPart(string text, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(content));
            }

            return SeparatorPartCore(text, content);
        }

        private static RoutePatternSeparatorPart SeparatorPartCore(string text, string content)
        {
            return new RoutePatternSeparatorPart(text, content);
        }

        public static RoutePatternParameterPart ParameterPart(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(name));
            }

            if (name.IndexOfAny(RoutePatternParser.InvalidParameterNameChars) >= 0)
            {
                throw new ArgumentException(Resources.FormatTemplateRoute_InvalidParameterName(name));
            }

            return ParameterPartCore(
                text: null,
                name: name,
                @default: null,
                kind: RoutePatternParameterKind.Standard,
                constraints: Array.Empty<RoutePatternConstraintReference>());
        }

        public static RoutePatternParameterPart ParameterPart(string text, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(name));
            }

            if (name.IndexOfAny(RoutePatternParser.InvalidParameterNameChars) >= 0)
            {
                throw new ArgumentException(Resources.FormatTemplateRoute_InvalidParameterName(name));
            }

            return ParameterPartCore(
                text: text,
                name: name,
                @default: null,
                kind: RoutePatternParameterKind.Standard,
                constraints: Array.Empty<RoutePatternConstraintReference>());
        }

        public static RoutePatternParameterPart ParameterPart(string name, object @default)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(name));
            }

            if (name.IndexOfAny(RoutePatternParser.InvalidParameterNameChars) >= 0)
            {
                throw new ArgumentException(Resources.FormatTemplateRoute_InvalidParameterName(name));
            }

            return ParameterPartCore(
                text: null,
                name: name,
                @default: @default,
                kind: RoutePatternParameterKind.Standard,
                constraints: Array.Empty<RoutePatternConstraintReference>());
        }

        public static RoutePatternParameterPart ParameterPart(string text, string name, object @default)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(name));
            }

            if (name.IndexOfAny(RoutePatternParser.InvalidParameterNameChars) >= 0)
            {
                throw new ArgumentException(Resources.FormatTemplateRoute_InvalidParameterName(name));
            }

            return ParameterPartCore(
                text: text,
                name: name,
                @default: @default,
                kind: RoutePatternParameterKind.Standard,
                constraints: Array.Empty<RoutePatternConstraintReference>());
        }

        public static RoutePatternParameterPart ParameterPart(
            string name,
            object @default,
            RoutePatternParameterKind kind)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(name));
            }

            if (name.IndexOfAny(RoutePatternParser.InvalidParameterNameChars) >= 0)
            {
                throw new ArgumentException(Resources.FormatTemplateRoute_InvalidParameterName(name));
            }

            if (@default != null && kind == RoutePatternParameterKind.Optional)
            {
                throw new ArgumentNullException(Resources.TemplateRoute_OptionalCannotHaveDefaultValue, nameof(kind));
            }

            return ParameterPartCore(
                text: null,
                name: name,
                @default: @default,
                kind: kind,
                constraints: Array.Empty<RoutePatternConstraintReference>());
        }

        public static RoutePatternParameterPart ParameterPart(
            string text,
            string name,
            object @default,
            RoutePatternParameterKind kind)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(name));
            }

            if (name.IndexOfAny(RoutePatternParser.InvalidParameterNameChars) >= 0)
            {
                throw new ArgumentException(Resources.FormatTemplateRoute_InvalidParameterName(name));
            }

            if (@default != null && kind == RoutePatternParameterKind.Optional)
            {
                throw new ArgumentNullException(Resources.TemplateRoute_OptionalCannotHaveDefaultValue, nameof(kind));
            }

            return ParameterPartCore(
                text: text,
                name: name,
                @default: @default,
                kind: kind,
                constraints: Array.Empty<RoutePatternConstraintReference>());
        }

        public static RoutePatternParameterPart ParameterPart(
            string name,
            object @default,
            RoutePatternParameterKind kind,
            IEnumerable<RoutePatternConstraintReference> constraints)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(name));
            }

            if (name.IndexOfAny(RoutePatternParser.InvalidParameterNameChars) >= 0)
            {
                throw new ArgumentException(Resources.FormatTemplateRoute_InvalidParameterName(name));
            }

            if (@default != null && kind == RoutePatternParameterKind.Optional)
            {
                throw new ArgumentNullException(Resources.TemplateRoute_OptionalCannotHaveDefaultValue, nameof(kind));
            }

            if (constraints == null)
            {
                throw new ArgumentNullException(nameof(constraints));
            }

            return ParameterPartCore(
                text: null,
                name: name,
                @default: @default,
                kind: kind,
                constraints: constraints);
        }

        public static RoutePatternParameterPart ParameterPart(
            string text,
            string name,
            object @default,
            RoutePatternParameterKind kind,
            IEnumerable<RoutePatternConstraintReference> constraints)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(name));
            }

            if (name.IndexOfAny(RoutePatternParser.InvalidParameterNameChars) >= 0)
            {
                throw new ArgumentException(Resources.FormatTemplateRoute_InvalidParameterName(name));
            }

            if (@default != null && kind == RoutePatternParameterKind.Optional)
            {
                throw new ArgumentNullException(Resources.TemplateRoute_OptionalCannotHaveDefaultValue, nameof(kind));
            }

            if (constraints == null)
            {
                throw new ArgumentNullException(nameof(constraints));
            }

            return ParameterPartCore(
                text: text,
                name: name,
                @default: @default,
                kind: kind,
                constraints: constraints);
        }

        public static RoutePatternParameterPart ParameterPart(
            string name,
            object @default,
            RoutePatternParameterKind kind,
            params RoutePatternConstraintReference[] constraints)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(name));
            }

            if (name.IndexOfAny(RoutePatternParser.InvalidParameterNameChars) >= 0)
            {
                throw new ArgumentException(Resources.FormatTemplateRoute_InvalidParameterName(name));
            }

            if (@default != null && kind == RoutePatternParameterKind.Optional)
            {
                throw new ArgumentNullException(Resources.TemplateRoute_OptionalCannotHaveDefaultValue, nameof(kind));
            }

            if (constraints == null)
            {
                throw new ArgumentNullException(nameof(constraints));
            }

            return ParameterPartCore(
                text: null,
                name: name,
                @default: @default,
                kind: kind,
                constraints: constraints);
        }

        public static RoutePatternParameterPart ParameterPart(
            string text,
            string name,
            object @default,
            RoutePatternParameterKind kind,
            params RoutePatternConstraintReference[] constraints)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(name));
            }

            if (name.IndexOfAny(RoutePatternParser.InvalidParameterNameChars) >= 0)
            {
                throw new ArgumentException(Resources.FormatTemplateRoute_InvalidParameterName(name));
            }

            if (@default != null && kind == RoutePatternParameterKind.Optional)
            {
                throw new ArgumentNullException(Resources.TemplateRoute_OptionalCannotHaveDefaultValue, nameof(kind));
            }

            if (constraints == null)
            {
                throw new ArgumentNullException(nameof(constraints));
            }

            return ParameterPartCore(
                text: text,
                name: name,
                @default: @default,
                kind: kind,
                constraints: constraints);
        }

        private static RoutePatternParameterPart ParameterPartCore(
            string text,
            string name,
            object @default,
            RoutePatternParameterKind kind,
            IEnumerable<RoutePatternConstraintReference> constraints)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(name));
            }

            if (name.IndexOfAny(RoutePatternParser.InvalidParameterNameChars) >= 0)
            {
                throw new ArgumentException(Resources.FormatTemplateRoute_InvalidParameterName(name));
            }

            return new RoutePatternParameterPart(text, name, @default, kind, constraints.ToArray());
        }

        public static RoutePatternConstraintReference Constraint(string name, object constraint)
        {
            // Similar to RouteConstraintBuilder
            if (constraint is IRouteConstraint routeConstraint)
            {
                return ConstraintCore(name, routeConstraint);
            }
            else if (constraint is string content)
            {
                return ConstraintCore(name, new RegexRouteConstraint("^(" + content + ")$"));
            }
            else
            {
                throw new InvalidOperationException(Resources.FormatConstraintMustBeStringOrConstraint(
                    name,
                    constraint,
                    typeof(IRouteConstraint)));
            }
        }

        public static RoutePatternConstraintReference Constraint(string name, IRouteConstraint constraint)
        {
            if (constraint == null)
            {
                throw new ArgumentNullException(nameof(constraint));
            }

            return ConstraintCore(name, constraint);
        }

        public static RoutePatternConstraintReference Constraint(string name, string constraint)
        {
            if (string.IsNullOrEmpty(constraint))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(constraint));
            }

            return ConstraintCore(null, name, constraint);
        }

        public static RoutePatternConstraintReference Constraint(string text, string name, string constraint)
        {
            if (string.IsNullOrEmpty(constraint))
            {
                throw new ArgumentException(Resources.Argument_NullOrEmpty, nameof(constraint));
            }

            return ConstraintCore(text, name, constraint);
        }

        private static RoutePatternConstraintReference ConstraintCore(string name, IRouteConstraint constraint)
        {
            return new RoutePatternConstraintReference(name, constraint);
        }

        private static RoutePatternConstraintReference ConstraintCore(string text, string name, string constraint)
        {
            return new RoutePatternConstraintReference(text, name, constraint);
        }
    }
}
