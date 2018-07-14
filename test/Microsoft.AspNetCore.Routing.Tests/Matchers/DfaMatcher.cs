// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.EndpointConstraints;

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

            var path = httpContext.Request.Path.Value;
            var buffer = stackalloc PathSegment[FastPathTokenizer.DefaultSegmentCount];
            var count = FastPathTokenizer.Tokenize(path, buffer, FastPathTokenizer.DefaultSegmentCount);
            var segments = new ReadOnlySpan<PathSegment>((void*)buffer, count);

            var candidates = SelectCandidates(path, segments);

            var matches = new List<(Endpoint, RouteValueDictionary)>();

            // This code ignores groups for right now.
            for (var i = 0; i < candidates.Candidates.Length; i++)
            {
                var candidate = candidates.Candidates[i];
                var values = new RouteValueDictionary();
                var parameters = candidate.Parameters;
                if (parameters != null)
                {
                    for (var j = 0; j < parameters.Length; j++)
                    {
                        var parameter = parameters[j];
                        if (parameter != null && buffer[j].Length == 0)
                        {
                            goto notmatch;
                        }
                        else if (parameter != null)
                        {
                            var value = path.Substring(buffer[j].Start, buffer[j].Length);
                            values.Add(parameter, value);
                        }
                    }
                }

                // This is some super super temporary code so we can pass the benchmarks
                // that do HTTP method matching.
                var httpMethodConstraint = candidate.Endpoint.Metadata.GetMetadata<HttpMethodEndpointConstraint>();
                if (httpMethodConstraint != null && !MatchHttpMethod(httpContext.Request.Method, httpMethodConstraint))
                {
                    goto notmatch;
                }

                matches.Add((candidate.Endpoint, values));

                notmatch: ;
            }
            
            feature.Endpoint = matches.Count == 0 ? null : matches[0].Item1;
            feature.Values = matches.Count == 0 ? null : matches[0].Item2;

            return Task.CompletedTask;
        }

        // This is some super super temporary code so we can pass the benchmarks
        // that do HTTP method matching.
        private bool MatchHttpMethod(string httpMethod, HttpMethodEndpointConstraint constraint)
        {
            foreach (var supportedHttpMethod in constraint.HttpMethods)
            {
                if (string.Equals(supportedHttpMethod, httpMethod, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public CandidateSet SelectCandidates(string path, ReadOnlySpan<PathSegment> segments)
        {
            var states = _states;
            var current = 0;

            for (var i = 0; i < segments.Length; i++)
            {
                current = states[current].Transitions.GetDestination(path, segments[i]);
            }

            return states[current].Matches;
        }

        public struct State
        {
            public bool IsAccepting;
            public CandidateSet Matches;
            public JumpTable Transitions;
        }
    }
}
