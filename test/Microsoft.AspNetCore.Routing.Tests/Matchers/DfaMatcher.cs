// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal class DfaMatcher : Matcher
    {
        private readonly State[] _states;

        public DfaMatcher(State[] states)
        {
            _states = states;
        }

        public unsafe override Task MatchAsync(HttpContext httpContext, IEndpointFeature feature)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (feature == null)
            {
                throw new ArgumentNullException(nameof(feature));
            }

            var states = _states;

            var path = httpContext.Request.Path.Value;
            var buffer = stackalloc PathSegment[32];
            var count = FastPathTokenizer.Tokenize(path, buffer, 32);
            var segments = new ReadOnlySpan<PathSegment>((void*)buffer, count);

            var node = FindMatchingNode(path, segments);

            var set = new CandidateSet()
            {
                Path = path,
                Candidates = states[node].Candidates,
                CandidateGroups = states[node].CandidateGroups,
                Segments = segments,
                Values = new RouteValueDictionary[states[node].Candidates.Length],
            };
            var match = SelectCandidate(set);

            if (match >= 0)
            {
                feature.Endpoint = set.Candidates[match].Endpoint;
                feature.Values = set.Values[match];
            }

            return Task.CompletedTask;
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

        private int SelectCandidate(CandidateSet set)
        {
            var offset = 0;
            for (var i = 0; i < set.CandidateGroups.Length; i++)
            {
                var groupLength = set.CandidateGroups[i];
                for (var j = offset; j < offset + groupLength; j++)
                {
                    var values = new RouteValueDictionary();
                    set.Values[j] = values;

                    var match = true;
                    var segments = set.Candidates[j].Segments;
                    for (var k = 0; k < segments.Length; k++)
                    {
                        match |=
                            segments[k] == null ||
                            segments[k].Process(values, set.Path.AsSpan(set.Segments[k].Start, set.Segments[k].Length));
                    }

                    if (match)
                    {
                        return j;
                    }
                }

                offset += groupLength;
            }

            return -1;
        }

        public readonly struct Candidate
        {
            public Candidate(Endpoint endpoint, SegmentProcesser[] segments)
            {
                Endpoint = endpoint;
                Segments = segments;
            }

            public readonly Endpoint Endpoint;
            public readonly SegmentProcesser[] Segments;
        }

        public abstract class SegmentProcesser
        {
            public abstract bool Process(RouteValueDictionary values, ReadOnlySpan<char> segment);
        }

        public sealed class ParameterSegmentProcessor : SegmentProcesser
        {
            private readonly string _name;

            public ParameterSegmentProcessor(string name)
            {
                _name = name;
            }

            public override bool Process(RouteValueDictionary values, ReadOnlySpan<char> segment)
            {
                values[_name] = segment.ToString();
                return true;
            }
        }

        public ref struct CandidateSet
        {
            public string Path;
            public ReadOnlySpan<PathSegment> Segments;
            public ReadOnlySpan<Candidate> Candidates;
            public ReadOnlySpan<int> CandidateGroups;
            public Span<RouteValueDictionary> Values;
        }

        public struct State
        {
            public Candidate[] Candidates;
            public int[] CandidateGroups;
            public JumpTable Transitions;
        }
    }
}
