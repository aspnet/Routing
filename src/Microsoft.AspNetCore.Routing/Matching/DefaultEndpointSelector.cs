// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing.Matching
{
    internal class DefaultEndpointSelector : EndpointSelector
    {
        private readonly IEndpointSelectorPolicy[] _selectorPolicies;
        
        public DefaultEndpointSelector(IEnumerable<MatcherPolicy> matcherPolicies)
        {
            if (matcherPolicies == null)
            {
                throw new ArgumentNullException(nameof(matcherPolicies));
            }

            _selectorPolicies = matcherPolicies.OrderBy(p => p.Order).OfType<IEndpointSelectorPolicy>().ToArray();
        }

        public override Task SelectAsync(
            HttpContext httpContext,
            IEndpointFeature feature,
            ReadOnlyMemory<CandidateState> candidateSet)
        {
            var selectorPolicies = _selectorPolicies;
            for (var i = 0; i < selectorPolicies.Length; i++)
            {
                selectorPolicies[i].Apply(httpContext, candidateSet);
            }

            MatcherEndpoint endpoint = null;
            RouteValueDictionary values = null;
            int? foundScore = null;

            var span = candidateSet.Span;
            for (var i = 0; i < span.Length; i++)
            {
                var isValid = span[i].IsValidCandidate;
                if (isValid && foundScore == null)
                {
                    // This is the first match we've seen - speculatively assign it.
                    endpoint = span[i].Endpoint;
                    values = span[i].Values;
                    foundScore = span[i].Score;
                }
                else if (isValid && foundScore < span[i].Score)
                {
                    // This candidate is lower priority than the one we've seen
                    // so far, we can stop.
                    //
                    // Don't worry about the 'null < state.Score' case, it returns false.
                    break;
                }
                else if (isValid && foundScore == span[i].Score)
                {
                    // This is the second match we've found of the same score, so there 
                    // must be an ambiguity.
                    //
                    // Don't worry about the 'null == state.Score' case, it returns false.

                    ReportAmbiguity(span);

                    // Unreachable, ReportAmbiguity always throws.
                    throw new NotSupportedException();
                }
            }

            if (endpoint != null)
            {
                feature.Endpoint = endpoint;
                feature.Invoker = endpoint.Invoker;
                feature.Values = values;
            }

            return Task.CompletedTask;
        }

        private static void ReportAmbiguity(ReadOnlySpan<CandidateState> candidates)
        {
            // If we get here it's the result of an ambiguity - we're OK with this
            // being a littler slower and more allocatey.
            var matches = new List<MatcherEndpoint>();
            for (var i = 0; i < candidates.Length; i++)
            {
                if (candidates[i].IsValidCandidate)
                {
                    matches.Add(candidates[i].Endpoint);
                }
            }

            var message = Resources.FormatAmbiguousEndpoints(
                Environment.NewLine,
                string.Join(Environment.NewLine, matches.Select(e => e.DisplayName)));
            throw new AmbiguousMatchException(message);
        }
    }
}
