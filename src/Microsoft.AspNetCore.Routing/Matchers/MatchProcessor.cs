// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal abstract class MatchProcessor
    {
        public abstract bool Process(
            HttpContext httpContext,
            string path,
            ReadOnlySpan<PathSegment> segments,
            RouteValueDictionary values);
    }
}
