// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal ref struct CandidateSet
    {
        public string Path;
        public ReadOnlySpan<PathSegment> Segments;
        public ReadOnlySpan<Candidate> Candidates;
        public ReadOnlySpan<int> CandidateIndices;
        public ReadOnlySpan<int> CandidateGroups;
        public Span<RouteValueDictionary> Values;

        public CandidateSet(string path, ReadOnlySpan<PathSegment> segments)
        {
            Path = path;
            Segments = segments;
            Candidates = Array.Empty<Candidate>();
            CandidateIndices = Array.Empty<int>();
            CandidateGroups = Array.Empty<int>();
            Values = Array.Empty<RouteValueDictionary>();
        }
    }
}
