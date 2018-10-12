// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Microsoft.AspNetCore.Routing.Template
{
    [DebuggerDisplay("{DebuggerToString()}")]
    public class TemplateSegment
    {
        public TemplateSegment()
        {
            Parts = new List<TemplatePart>();
        }

        public TemplateSegment(RoutePatternPathSegment other)
        {
            var partCount = other.Parts.Count;
            Parts = new List<TemplatePart>(partCount);
            for (var i = 0; i < partCount; i++)
            {
                Parts.Add(new TemplatePart(other.Parts[i]));
            }
        }

        public bool IsSimple => Parts.Count == 1;

        public List<TemplatePart> Parts { get; }

        internal string DebuggerToString()
        {
            return string.Join(string.Empty, Parts.Select(p => p.DebuggerToString()));
        }

        public RoutePatternPathSegment ToRoutePatternPathSegment()
        {
            var partCount = Parts.Count;
            var patternParts = new RoutePatternPart[partCount];
            for (var i = 0; i < partCount; i++)
            {
                patternParts[i] = Parts[i].ToRoutePatternPart();
            }

            return RoutePatternFactory.Segment(patternParts);
        }
    }
}
