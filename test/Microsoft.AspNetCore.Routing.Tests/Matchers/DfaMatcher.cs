// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
            var current = 0;

            var path = httpContext.Request.Path.Value;
            var segments = stackalloc PathSegment[32];
            var count = FastPathTokenizer.Tokenize(path, segments, 32);
            
            for (var i = 0; i < count; i++)
            {
                ref var segment = ref segments[i];
                var span = path.AsSpan(segment.Start, segment.Length);
                current = states[current].Transitions.GetDestination(span);
            }

            var matches = new List<(Endpoint, RouteValueDictionary)>();
            var candidates = states[current].Matches;

            ProcessMatches(path, matches, candidates, segments);
            
            feature.Endpoint = matches.Count == 0 ? null : matches[0].Item1;
            feature.Values = matches.Count == 0 ? null : matches[0].Item2;

            return Task.CompletedTask;
        }

        private unsafe void ProcessMatches(string path, List<(Endpoint, RouteValueDictionary)> matches, Candidate[] candidates, PathSegment* segments)
        {
            for (var i = 0; i < candidates.Length; i++)
            {
                var values = new RouteValueDictionary();
                var parameters = candidates[i].Parameters;
                if (parameters != null)
                {
                    for (var j = 0; j < parameters.Length; j++)
                    {
                        var parameter = parameters[j];
                        if (parameter != null && segments[j].Length == 0)
                        {
                            goto notmatch;
                        }
                        else if (parameter != null)
                        {
                            var value = path.Substring(segments[j].Start, segments[j].Length);
                            values.Add(parameter, value);
                        }
                    }
                }

                matches.Add((candidates[i].Endpoint, values));

                notmatch:;
            }
        }

        public struct State
        {
            public bool IsAccepting;
            public Candidate[] Matches;
            public JumpTable Transitions;
        }

        public struct Candidate
        {
            public Endpoint Endpoint;
            public string[] Parameters;
        }
    }
}
