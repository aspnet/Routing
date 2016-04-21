// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing.Template;

namespace Microsoft.AspNetCore.Routing.Tree
{
    public class UrlMatchingNode
    {
        public UrlMatchingNode(int length)
        {
            Length = length;

            Matches = new List<KeyValuePair<InboundRouteEntry, TemplateMatcher>>();
            Literals = new Dictionary<string, UrlMatchingNode>(StringComparer.OrdinalIgnoreCase);
        }

        public int Length { get; }

        public bool IsCatchAll { get; set; }

        // These entries are sorted by precedence then template
        public List<KeyValuePair<InboundRouteEntry, TemplateMatcher>> Matches { get; }

        public Dictionary<string, UrlMatchingNode> Literals { get; }

        public UrlMatchingNode ConstrainedParameters { get; set; }

        public UrlMatchingNode Parameters { get; set; }

        public UrlMatchingNode ConstrainedCatchAlls { get; set; }

        public UrlMatchingNode CatchAlls { get; set; }
    }
}
