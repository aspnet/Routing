// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    // Just like TechEmpower Plaintext
    public partial class MatcherSingleEntryBenchmark : MatcherBenchmarkBase
    {
        private const int SampleCount = 100;

        private BarebonesMatcher _baseline;
        private Matcher _dfa;
        private Matcher _route;
        private Matcher _tree;

        private EndpointFeature _feature;

        [GlobalSetup]
        public void Setup()
        {
            _endpoints = new MatcherEndpoint[1];
            _endpoints[0] = CreateEndpoint("/plaintext");

            _requests = new HttpContext[1];
            _requests[0] = new DefaultHttpContext();
            _requests[0].RequestServices = CreateServices();
            _requests[0].Request.Path = "/plaintext";

            _baseline = (BarebonesMatcher)SetupMatcher(new BarebonesMatcherBuilder());
            _dfa = SetupMatcher(new DfaMatcherBuilder());
            _route = SetupMatcher(new RouteMatcherBuilder());
            _tree = SetupMatcher(new TreeRouterMatcherBuilder());

            _feature = new EndpointFeature();
        }

        private Matcher SetupMatcher(MatcherBuilder builder)
        {
            builder.AddEndpoint(_endpoints[0]);
            return builder.Build();
        }

        [Benchmark(Baseline = true)]
        public async Task Baseline()
        {
            var feature = _feature;
            var httpContext = _requests[0];

            await _baseline.MatchAsync(httpContext, feature);
            Validate(httpContext, _endpoints[0], feature.Endpoint);
        }

        [Benchmark]
        public async Task Dfa()
        {
            var feature = _feature;
            var httpContext = _requests[0];

            await _dfa.MatchAsync(httpContext, feature);
            Validate(httpContext, _endpoints[0], feature.Endpoint);
        }

        [Benchmark]
        public async Task LegacyTreeRouter()
        {
            var feature = _feature;

            var httpContext = _requests[0];

            // This is required to make the legacy router implementation work with dispatcher.
            httpContext.Features.Set<IEndpointFeature>(feature);

            await _tree.MatchAsync(httpContext, feature);
            Validate(httpContext, _endpoints[0], feature.Endpoint);
        }

        [Benchmark]
        public async Task LegacyRouter()
        {
            var feature = _feature;
            var httpContext = _requests[0];

            // This is required to make the legacy router implementation work with dispatcher.
            httpContext.Features.Set<IEndpointFeature>(feature);

            await _route.MatchAsync(httpContext, feature);
            Validate(httpContext, _endpoints[0], feature.Endpoint);
        }
    }
}