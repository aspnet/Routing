// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    // A test-only matcher implementation - used as a baseline for more compilated
    // perf tests. The idea with this matcher is that we can cheat on the requirements
    // to establish a lower bound for perf comparisons.
    internal class BarebonesMatcher : Matcher
    {
        public readonly InnerMatcher[] _matchers;

        public BarebonesMatcher(InnerMatcher[] matchers)
        {
            _matchers = matchers;
        }

        public override Task MatchAsync(HttpContext httpContext, IEndpointFeature feature)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (feature == null)
            {
                throw new ArgumentNullException(nameof(feature));
            }

            for (var i = 0; i < _matchers.Length; i++)
            {
                if (_matchers[i].TryMatch(httpContext.Request.Path.Value))
                {
                    feature.Endpoint = _matchers[i]._endpoint;
                    feature.Values = new RouteValueDictionary();
                }
            }

            return Task.CompletedTask;
        }

        public sealed class InnerMatcher : Matcher
        {
            private readonly string[] _segments;
            public readonly MatcherEndpoint _endpoint;
            private readonly CandidateSet _candidates;

            public InnerMatcher(string[] segments, MatcherEndpoint endpoint)
            {
                _segments = segments;
                _endpoint = endpoint;

                _candidates = new CandidateSet(
                    new Candidate[] { new Candidate(endpoint), },

                    // Single candidate group that contains one entry.
                    CandidateSet.MakeGroups(new[] { 1 }));
            }

            public bool TryMatch(string path)
            {
                var segment = 0;

                var start = 1; // PathString always has a leading slash
                var end = 0;
                while ((end = path.IndexOf('/', start)) >= 0)
                {
                    var comparand = _segments.Length > segment ? _segments[segment] : null;
                    if ((comparand == null && end - start == 0) ||
                        (comparand != null &&
                            (comparand.Length != end - start ||
                            string.Compare(
                                path,
                                start,
                                comparand,
                                0,
                                comparand.Length,
                                StringComparison.OrdinalIgnoreCase) != 0)))
                    {
                        return false;
                    }

                    start = end + 1;
                    segment++;
                }

                // residue
                var length = path.Length - start;
                if (length > 0)
                {
                    var comparand = _segments.Length > segment ? _segments[segment] : null;
                    if (comparand != null &&
                        (comparand.Length != length ||
                        string.Compare(
                            path,
                            start,
                            comparand,
                            0,
                            comparand.Length,
                            StringComparison.OrdinalIgnoreCase) != 0))
                    {
                        return false;
                    }

                    segment++;
                }

                return segment == _segments.Length;
            }

            internal CandidateSet SelectCandidates(string path, ReadOnlySpan<PathSegment> segments)
            {
                if (TryMatch(path))
                {
                    return _candidates;
                }

                return CandidateSet.Empty;
            }

            public override Task MatchAsync(HttpContext httpContext, IEndpointFeature feature)
            {
                if (TryMatch(httpContext.Request.Path.Value))
                {
                    feature.Endpoint = _endpoint;
                    feature.Values = new RouteValueDictionary();
                }

                return Task.CompletedTask;
            }
        }
    }
}