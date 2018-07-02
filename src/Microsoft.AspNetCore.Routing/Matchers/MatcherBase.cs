// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal abstract class MatcherBase : Matcher
    {
        protected abstract void SelectCandidates(HttpContext httpContext, ref CandidateSet candidates);

        public sealed unsafe override Task MatchAsync(HttpContext httpContext, IEndpointFeature feature)
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
            var buffer = stackalloc PathSegment[32];
            var count = FastPathTokenizer.Tokenize(path, buffer, 32);
            var segments = new ReadOnlySpan<PathSegment>((void*)buffer, count);

            var candidates = new CandidateSet(path, segments);
            SelectCandidates(httpContext, ref candidates);
            return SelectBestCandidateAsync(httpContext, candidates, feature);
        }

        private Task SelectBestCandidateAsync(
            HttpContext httpContext,
            CandidateSet candidates,
            IEndpointFeature feature)
        {
            candidates.Values = new RouteValueDictionary[candidates.CandidateIndices.Length];

            var offset = 0;
            for (var i = 0; i < candidates.CandidateGroups.Length; i++)
            {
                var groupLength = candidates.CandidateGroups[i];
                for (var j = offset; j < offset + groupLength; j++)
                {
                    var values = new RouteValueDictionary();
                    candidates.Values[j] = values;

                    var match = true;
                    var processors = candidates.Candidates[j].Processors;
                    for (var k = 0; k < processors.Length; k++)
                    {
                        var processor = processors[k];
                        match |=  processors[k].Process(
                            httpContext,
                            candidates.Path,
                            candidates.Segments,
                            values);
                    }

                    if (match)
                    {
                        feature.Endpoint = candidates.Candidates[j].Endpoint;
                        feature.Values = candidates.Values[j];
                        return Task.CompletedTask;
                    }
                }

                offset += groupLength;
            }

            return Task.CompletedTask;
        }
    }
}
