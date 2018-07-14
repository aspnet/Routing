// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    public class MatcherSelectCandidatesSmallEntryCountBenchmark : MatcherBenchmarkBase
    {
        private TrivialMatcher _baseline;
        private DfaMatcher _dfa;

        private EndpointFeature _feature;

        [GlobalSetup]
        public void Setup()
        {
            SetupEndpoints();

            SetupRequests();

            _baseline = (TrivialMatcher)SetupMatcher(new TrivialMatcherBuilder());
            _dfa = (DfaMatcher)SetupMatcher(new DfaMatcherBuilder());

            _feature = new EndpointFeature();
        }

        private void SetupEndpoints()
        {
            _endpoints = new MatcherEndpoint[10];
            _endpoints[0] = CreateEndpoint("/another-really-cool-entry");
            _endpoints[1] = CreateEndpoint("/Some-Entry");
            _endpoints[2] = CreateEndpoint("/a/path/with/more/segments");
            _endpoints[3] = CreateEndpoint("/random/name");
            _endpoints[4] = CreateEndpoint("/random/name2");
            _endpoints[5] = CreateEndpoint("/random/name3");
            _endpoints[6] = CreateEndpoint("/random/name4");
            _endpoints[7] = CreateEndpoint("/plaintext1");
            _endpoints[8] = CreateEndpoint("/plaintext2");
            _endpoints[9] = CreateEndpoint("/plaintext");
        }

        private void SetupRequests()
        {
            _requests = new HttpContext[1];
            _requests[0] = new DefaultHttpContext();
            _requests[0].RequestServices = CreateServices();
            _requests[0].Request.Path = "/plaintext";
        }

        // For this case we're specifically targeting the last entry to hit 'worst case'
        // performance for the matchers that scale linearly.
        private Matcher SetupMatcher(MatcherBuilder builder)
        {
            builder.AddEndpoint(_endpoints[0]);
            builder.AddEndpoint(_endpoints[1]);
            builder.AddEndpoint(_endpoints[2]);
            builder.AddEndpoint(_endpoints[3]);
            builder.AddEndpoint(_endpoints[4]);
            builder.AddEndpoint(_endpoints[5]);
            builder.AddEndpoint(_endpoints[6]);
            builder.AddEndpoint(_endpoints[7]);
            builder.AddEndpoint(_endpoints[8]);
            builder.AddEndpoint(_endpoints[9]);
            return builder.Build();
        }

        [Benchmark(Baseline = true)]
        public unsafe void Baseline()
        {
            var httpContext = _requests[0];
            var path = httpContext.Request.Path.Value;
            var segments = new ReadOnlySpan<PathSegment>(Array.Empty<PathSegment>());

            var candidates = _baseline.SelectCandidates(path, segments);

            var endpoint = candidates.Candidates[0].Endpoint;
            Validate(_requests[0], _endpoints[9], endpoint);
        }

        [Benchmark]
        public unsafe void Dfa()
        {
            var httpContext = _requests[0];
            var path = httpContext.Request.Path.Value;
            var buffer = stackalloc PathSegment[FastPathTokenizer.DefaultSegmentCount];
            var count = FastPathTokenizer.Tokenize(path, buffer, FastPathTokenizer.DefaultSegmentCount);
            var segments = new ReadOnlySpan<PathSegment>((void*)buffer, count);

            var candidates = _dfa.SelectCandidates(path, segments);

            var endpoint = candidates.Candidates[0].Endpoint;
            Validate(_requests[0], _endpoints[9], endpoint);
        }
    }
}
