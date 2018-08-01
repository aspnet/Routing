﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Routing.Matching
{
    // Many of these are integration tests that exercise the system end to end,
    // so we're reusing the services here.
    public class DfaMatcherTest
    {
        private MatcherEndpoint CreateEndpoint(string template, int order, object defaults = null, EndpointMetadataCollection metadata = null)
        {
            return new MatcherEndpoint(
                MatcherEndpoint.EmptyInvoker,
                RoutePatternFactory.Parse(template, defaults, constraints: null),
                new RouteValueDictionary(),
                order,
                metadata ?? EndpointMetadataCollection.Empty,
                template);
        }

        private Matcher CreateDfaMatcher(EndpointDataSource dataSource, EndpointSelector endpointSelector = null)
        {
            var serviceCollection = new ServiceCollection()
                .AddLogging()
                .AddOptions()
                .AddRouting();

            if (endpointSelector != null)
            {
                serviceCollection.AddSingleton<EndpointSelector>(endpointSelector);
            }

            var services = serviceCollection.BuildServiceProvider();

            var factory = services.GetRequiredService<MatcherFactory>();
            return Assert.IsType<DataSourceDependentMatcher>(factory.CreateMatcher(dataSource));
        }

        [Fact]
        public async Task MatchAsync_ValidRouteConstraint_EndpointMatched()
        {
            // Arrange
            var endpointDataSource = new DefaultEndpointDataSource(new List<Endpoint>
            {
                CreateEndpoint("/{p:int}", 0)
            });

            var matcher = CreateDfaMatcher(endpointDataSource);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/1";

            var endpointFeature = new EndpointFeature();

            // Act
            await matcher.MatchAsync(httpContext, endpointFeature);

            // Assert
            Assert.NotNull(endpointFeature.Endpoint);
        }

        [Fact]
        public async Task MatchAsync_InvalidRouteConstraint_NoEndpointMatched()
        {
            // Arrange
            var endpointDataSource = new DefaultEndpointDataSource(new List<Endpoint>
            {
                CreateEndpoint("/{p:int}", 0)
            });

            var matcher = CreateDfaMatcher(endpointDataSource);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/One";

            var endpointFeature = new EndpointFeature();

            // Act
            await matcher.MatchAsync(httpContext, endpointFeature);

            // Assert
            Assert.Null(endpointFeature.Endpoint);
        }

        [Fact]
        public async Task MatchAsync_DuplicateTemplatesAndDifferentOrder_LowerOrderEndpointMatched()
        {
            // Arrange
            var higherOrderEndpoint = CreateEndpoint("/Teams", 1);
            var lowerOrderEndpoint = CreateEndpoint("/Teams", 0);

            var endpointDataSource = new DefaultEndpointDataSource(new List<Endpoint>
            {
                higherOrderEndpoint,
                lowerOrderEndpoint
            });

            var matcher = CreateDfaMatcher(endpointDataSource);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/Teams";

            var endpointFeature = new EndpointFeature();

            // Act
            await matcher.MatchAsync(httpContext, endpointFeature);

            // Assert
            Assert.Equal(lowerOrderEndpoint, endpointFeature.Endpoint);
        }

        [Fact]
        public async Task MatchAsync_MultipleMatches_EndpointSelectorCalled()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/Teams", 0);
            var endpoint2 = CreateEndpoint("/Teams", 1);

            var endpointSelector = new Mock<EndpointSelector>();
            endpointSelector
                .Setup(s => s.SelectAsync(It.IsAny<HttpContext>(), It.IsAny<IEndpointFeature>(), It.IsAny<CandidateSet>()))
                .Callback<HttpContext, IEndpointFeature, CandidateSet>((c, f, cs) =>
                {
                    Assert.Equal(2, cs.Count);

                    Assert.Same(endpoint1, cs[0].Endpoint);
                    Assert.True(cs[0].IsValidCandidate);
                    Assert.Equal(0, cs[0].Score);
                    Assert.Empty(cs[0].Values);

                    Assert.Same(endpoint2, cs[1].Endpoint);
                    Assert.True(cs[1].IsValidCandidate);
                    Assert.Equal(1, cs[1].Score);
                    Assert.Empty(cs[1].Values);

                    f.Endpoint = endpoint2;
                })
                .Returns(Task.CompletedTask);

            var endpointDataSource = new DefaultEndpointDataSource(new List<Endpoint>
            {
                endpoint1,
                endpoint2
            });

            var matcher = CreateDfaMatcher(endpointDataSource, endpointSelector.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/Teams";

            var endpointFeature = new EndpointFeature();

            // Act
            await matcher.MatchAsync(httpContext, endpointFeature);

            // Assert
            Assert.Equal(endpoint2, endpointFeature.Endpoint);
        }
    }
}
