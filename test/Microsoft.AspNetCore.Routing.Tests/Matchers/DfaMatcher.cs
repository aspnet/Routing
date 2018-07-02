// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal class DfaMatcher : MatcherBase
    {
        private readonly State[] _states;

        public DfaMatcher(State[] states)
        {
            _states = states;
        }

        protected override void SelectCandidates(HttpContext httpContext, ref CandidateSet candidates)
        {
            var states = _states;
            var node = FindMatchingNode(candidates.Path, candidates.Segments);

            candidates.Candidates = states[node].Candidates;
            candidates.CandidateIndices = states[node].CandidateIndices;
            candidates.CandidateGroups = states[node].CandidateGroups;
        }

        private int FindMatchingNode(string path, ReadOnlySpan<PathSegment> segments)
        {
            var states = _states;
            var current = 0;
            for (var i = 0; i < segments.Length; i++)
            {
                current = states[current].Transitions.GetDestination(path, segments[i]);
            }

            return current;
        }

        public struct State
        {
            public Candidate[] Candidates;
            public int[] CandidateIndices;
            public int[] CandidateGroups;
            public JumpTable Transitions;
        }
    }
}
