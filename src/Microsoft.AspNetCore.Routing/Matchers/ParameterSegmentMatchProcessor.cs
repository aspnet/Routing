// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal sealed class ParameterSegmentMatchProcessor : MatchProcessor
    {
        private readonly int _segment;
        private readonly string _name;

        public ParameterSegmentMatchProcessor(int segment, string name)
        {
            _segment = segment;
            _name = name;
        }

        public override bool Process(
            HttpContext httpContext,
            string path,
            ReadOnlySpan<PathSegment> segments,
            RouteValueDictionary values)
        {
            var segment = segments[_segment];
            values[_name] = path.Substring(segment.Start, segment.Length);
            return true;
        }
    }
}