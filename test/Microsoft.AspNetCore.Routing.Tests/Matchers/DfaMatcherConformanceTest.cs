﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    public class DfaMatcherConformanceTest : FullFeaturedMatcherConformanceTest
    {
        // See the comments in the base class. DfaMatcher fixes a long-standing bug
        // with catchall parameters and empty segments.
        public override async Task Quirks_CatchAllParameter(string template, string path, string[] keys, string[] values)
        {
            // Arrange
            var (matcher, endpoint) = CreateMatcher(template);
            var (httpContext, feature) = CreateContext(path);

            // Act
            await matcher.MatchAsync(httpContext, feature);

            // Assert
            MatcherAssert.AssertMatch(feature, endpoint, keys, values);
        }

        internal override Matcher CreateMatcher(params MatcherEndpoint[] endpoints)
        {
            var services = new ServiceCollection()
                .AddLogging()
                .AddOptions()
                .AddRouting()
                .BuildServiceProvider();

            var builder = services.GetRequiredService<DfaMatcherBuilder>();
            for (int i = 0; i < endpoints.Length; i++)
            {
                builder.AddEndpoint(endpoints[i]);
            }
            return builder.Build();
        }
    }
}
