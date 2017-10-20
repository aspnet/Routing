// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Dispatcher.Patterns;
using Other = Microsoft.AspNetCore.Dispatcher.Patterns.RoutePattern;

namespace Microsoft.AspNetCore.Routing.Template
{
    [DebuggerDisplay("{DebuggerToString()}")]
    public class RouteTemplate
    {
        private const string SeparatorString = "/";

        public RouteTemplate(Other other)
        {
            TemplateText = other.RawText;
            Segments = new List<TemplateSegment>(other.PathSegments.Select(p => new TemplateSegment(p)));
            Parameters = new List<TemplatePart>();
            for (var i = 0; i < Segments.Count; i++)
            {
                var segment = Segments[i];
                for (var j = 0; j < segment.Parts.Count; j++)
                {
                    var part = segment.Parts[j];
                    if (part.IsParameter)
                    {
                        Parameters.Add(part);
                    }
                }
            }
        }

        public RouteTemplate(string template, List<TemplateSegment> segments)
        {
            if (segments == null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            TemplateText = template;

            Segments = segments;

            Parameters = new List<TemplatePart>();
            for (var i = 0; i < segments.Count; i++)
            {
                var segment = Segments[i];
                for (var j = 0; j < segment.Parts.Count; j++)
                {
                    var part = segment.Parts[j];
                    if (part.IsParameter)
                    {
                        Parameters.Add(part);
                    }
                }
            }
        }

        public string TemplateText { get; }

        public IList<TemplatePart> Parameters { get; }

        public IList<TemplateSegment> Segments { get; }

        public TemplateSegment GetSegment(int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException();
            }

            return index >= Segments.Count ? null : Segments[index];
        }

        private string DebuggerToString()
        {
            return string.Join(SeparatorString, Segments.Select(s => s.DebuggerToString()));
        }

        /// <summary>
        /// Gets the parameter matching the given name.
        /// </summary>
        /// <param name="name">The name of the parameter to match.</param>
        /// <returns>The matching parameter or <c>null</c> if no parameter matches the given name.</returns>
        public TemplatePart GetParameter(string name)
        {
            for (var i = 0; i < Parameters.Count; i++)
            {
                var parameter = Parameters[i];
                if (string.Equals(parameter.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return parameter;
                }
            }

            return null;
        }

        /// <summary>
        /// Converts the <see cref="RouteTemplate"/> to the equivalent 
        /// <see cref="RoutePattern"/>
        /// </summary>
        /// <returns>A <see cref="RoutePattern"/>.</returns>
        public Other ToRoutePattern()
        {
            var builder = RoutePatternBuilder.Create(TemplateText);

            for (var i = 0; i < Segments.Count; i++)
            {
                var segment = Segments[i];

                var parts = new List<RoutePatternPart>();
                for (var  j = 0; j < segment.Parts.Count; j++)
                {
                    var part = segment.Parts[j];
                    if (part.IsLiteral && part.IsOptionalSeperator)
                    {
                        parts.Add(RoutePatternPart.CreateSeparator(part.Text));
                    }
                    else if (part.IsLiteral)
                    {
                        parts.Add(RoutePatternPart.CreateLiteral(part.Text));
                    }
                    else
                    {
                        var kind = part.IsCatchAll ? RoutePatternParameterKind.CatchAll : part.IsOptional ? RoutePatternParameterKind.Optional : RoutePatternParameterKind.Standard;
                        parts.Add(RoutePatternPart.CreateParameter(part.Name, part.DefaultValue, kind));
                    }
                }

                builder.AddPathSegment(parts.ToArray());
            }

            return builder.Build();
        }
    }
}
