﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing.EndpointFinders;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.AspNetCore.Routing.Matchers;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.TestObjects;
using Microsoft.AspNetCore.Routing.Tree;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Routing
{
    public class RouteValueBasedEndpointFinderTest
    {
        [Fact]
        public void GetOutboundMatches_GetsNamedMatchesFor_EndpointsHaving_IRouteNameMetadata()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/a");
            var endpoint2 = CreateEndpoint("/a", routeName: "named");

            // Act
            var finder = CreateEndpointFinder(endpoint1, endpoint2);

            // Assert
            Assert.NotNull(finder.AllMatches);
            Assert.Equal(2, finder.AllMatches.Count());
            Assert.NotNull(finder.NamedMatches);
            Assert.True(finder.NamedMatches.TryGetValue("named", out var namedMatches));
            var namedMatch = Assert.Single(namedMatches);
            var actual = Assert.IsType<MatcherEndpoint>(namedMatch.Match.Entry.Data);
            Assert.Same(endpoint2, actual);
        }

        [Fact]
        public void GetOutboundMatches_GroupsMultipleEndpoints_WithSameName()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/a");
            var endpoint2 = CreateEndpoint("/a", routeName: "named");
            var endpoint3 = CreateEndpoint("/b", routeName: "named");

            // Act
            var finder = CreateEndpointFinder(endpoint1, endpoint2, endpoint3);

            // Assert
            Assert.NotNull(finder.AllMatches);
            Assert.Equal(3, finder.AllMatches.Count());
            Assert.NotNull(finder.NamedMatches);
            Assert.True(finder.NamedMatches.TryGetValue("named", out var namedMatches));
            Assert.Equal(2, namedMatches.Count);
            Assert.Same(endpoint2, Assert.IsType<MatcherEndpoint>(namedMatches[0].Match.Entry.Data));
            Assert.Same(endpoint3, Assert.IsType<MatcherEndpoint>(namedMatches[1].Match.Entry.Data));
        }

        [Fact]
        public void GetOutboundMatches_GroupsMultipleEndpoints_WithSameName_IgnoringCase()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/a");
            var endpoint2 = CreateEndpoint("/a", routeName: "named");
            var endpoint3 = CreateEndpoint("/b", routeName: "NaMed");

            // Act
            var finder = CreateEndpointFinder(endpoint1, endpoint2, endpoint3);

            // Assert
            Assert.NotNull(finder.AllMatches);
            Assert.Equal(3, finder.AllMatches.Count());
            Assert.NotNull(finder.NamedMatches);
            Assert.True(finder.NamedMatches.TryGetValue("named", out var namedMatches));
            Assert.Equal(2, namedMatches.Count);
            Assert.Same(endpoint2, Assert.IsType<MatcherEndpoint>(namedMatches[0].Match.Entry.Data));
            Assert.Same(endpoint3, Assert.IsType<MatcherEndpoint>(namedMatches[1].Match.Entry.Data));
        }

        [Fact]
        public void GetOutboundMatches_DoesNotGetNamedMatchesFor_EndpointsHaving_INameMetadata()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/a");
            var endpoint2 = CreateEndpoint("/a", routeName: "named");
            var endpoint3 = CreateEndpoint(
                "/b",
                metadataCollection: new EndpointMetadataCollection(new[] { new NameMetadata("named") }));

            // Act
            var finder = CreateEndpointFinder(endpoint1, endpoint2);

            // Assert
            Assert.NotNull(finder.AllMatches);
            Assert.Equal(2, finder.AllMatches.Count());
            Assert.NotNull(finder.NamedMatches);
            Assert.True(finder.NamedMatches.TryGetValue("named", out var namedMatches));
            var namedMatch = Assert.Single(namedMatches);
            var actual = Assert.IsType<MatcherEndpoint>(namedMatch.Match.Entry.Data);
            Assert.Same(endpoint2, actual);
        }

        [Fact]
        public void EndpointDataSource_ChangeCallback_Refreshes_OutboundMatches()
        {
            // Arrange 1
            var endpoint1 = CreateEndpoint("/a");
            var dynamicDataSource = new DynamicEndpointDataSource(new[] { endpoint1 });
            var objectPoolProvider = new DefaultObjectPoolProvider();
            var objectPool = objectPoolProvider.Create(new UriBuilderContextPooledObjectPolicy());

            // Act 1
            var finder = new CustomRouteValuesBasedEndpointFinder(
                new CompositeEndpointDataSource(new[] { dynamicDataSource }),
                objectPool);

            // Assert 1
            Assert.NotNull(finder.AllMatches);
            var match = Assert.Single(finder.AllMatches);
            var actual = Assert.IsType<MatcherEndpoint>(match.Entry.Data);
            Assert.Same(endpoint1, actual);

            // Arrange 2
            var endpoint2 = CreateEndpoint("/b");

            // Act 2
            // Trigger change
            dynamicDataSource.AddEndpoint(endpoint2);

            // Arrange 2
            var endpoint3 = CreateEndpoint("/c");

            // Act 2
            // Trigger change
            dynamicDataSource.AddEndpoint(endpoint3);

            // Arrange 3
            var endpoint4 = CreateEndpoint("/d");

            // Act 3
            // Trigger change
            dynamicDataSource.AddEndpoint(endpoint4);

            // Assert 3
            Assert.NotNull(finder.AllMatches);
            Assert.Collection(
                finder.AllMatches,
                (m) =>
                {
                    actual = Assert.IsType<MatcherEndpoint>(m.Entry.Data);
                    Assert.Same(endpoint1, actual);
                },
                (m) =>
                {
                    actual = Assert.IsType<MatcherEndpoint>(m.Entry.Data);
                    Assert.Same(endpoint2, actual);
                },
                (m) =>
                {
                    actual = Assert.IsType<MatcherEndpoint>(m.Entry.Data);
                    Assert.Same(endpoint3, actual);
                },
                (m) =>
                {
                    actual = Assert.IsType<MatcherEndpoint>(m.Entry.Data);
                    Assert.Same(endpoint4, actual);
                });
        }

        [Fact]
        public void FindEndpoints_ReturnsEndpoint_WhenLookedUpByRouteName()
        {
            // Arrange
            var expected = CreateEndpoint(
                "api/orders/{id}",
                defaults: new { controller = "Orders", action = "GetById" },
                requiredValues: new { controller = "Orders", action = "GetById" },
                routeName: "OrdersApi");
            var finder = CreateEndpointFinder(expected);

            // Act
            var foundEndpoints = finder.FindEndpoints(
                new RouteValuesBasedEndpointFinderContext
                {
                    ExplicitValues = new RouteValueDictionary(new { id = 10 }),
                    AmbientValues = new RouteValueDictionary(new { controller = "Home", action = "Index" }),
                    RouteName = "OrdersApi"
                });

            // Assert
            var actual = Assert.Single(foundEndpoints);
            Assert.Same(expected, actual);
        }

        [Fact]
        public void FindEndpoints_AlwaysReturnsEndpointsByRouteName_IgnoringMissingRequiredParameterValues()
        {
            // Here 'id' is the required value. The endpoint finder would always return an endpoint by looking up
            // name only. Its the link generator which uses these endpoints finally to generate a link or not
            // based on the required parameter values being present or not.

            // Arrange
            var expected = CreateEndpoint(
                "api/orders/{id}",
                defaults: new { controller = "Orders", action = "GetById" },
                requiredValues: new { controller = "Orders", action = "GetById" },
                routeName: "OrdersApi");
            var finder = CreateEndpointFinder(expected);

            // Act
            var foundEndpoints = finder.FindEndpoints(
                new RouteValuesBasedEndpointFinderContext
                {
                    ExplicitValues = new RouteValueDictionary(),
                    AmbientValues = new RouteValueDictionary(),
                    RouteName = "OrdersApi"
                });

            // Assert
            var actual = Assert.Single(foundEndpoints);
            Assert.Same(expected, actual);
        }

        [Fact]
        public void GetOutboundMatches_DoesNotInclude_EndpointsWithSuppressLinkGenerationMetadata()
        {
            // Arrange
            var endpoint = CreateEndpoint(
                "/a",
                metadataCollection: new EndpointMetadataCollection(new[] { new SuppressLinkGenerationMetadata() }));

            // Act
            var finder = CreateEndpointFinder(endpoint);

            // Assert
            Assert.Empty(finder.AllMatches);
        }

        private CustomRouteValuesBasedEndpointFinder CreateEndpointFinder(params Endpoint[] endpoints)
        {
            return CreateEndpointFinder(new DefaultEndpointDataSource(endpoints));
        }

        private CustomRouteValuesBasedEndpointFinder CreateEndpointFinder(params EndpointDataSource[] endpointDataSources)
        {
            var objectPoolProvider = new DefaultObjectPoolProvider();
            var objectPool = objectPoolProvider.Create(new UriBuilderContextPooledObjectPolicy());

            return new CustomRouteValuesBasedEndpointFinder(
                new CompositeEndpointDataSource(endpointDataSources),
                objectPool);
        }

        private MatcherEndpoint CreateEndpoint(
            string template,
            object defaults = null,
            object requiredValues = null,
            int order = 0,
            string routeName = null,
            EndpointMetadataCollection metadataCollection = null)
        {
            if (metadataCollection == null)
            {
                metadataCollection = EndpointMetadataCollection.Empty;
                if (!string.IsNullOrEmpty(routeName))
                {
                    metadataCollection = new EndpointMetadataCollection(new[] { new RouteNameMetadata(routeName) });
                }
            }

            return new MatcherEndpoint(
                MatcherEndpoint.EmptyInvoker,
                RoutePatternFactory.Parse(template, defaults, constraints: null),
                new RouteValueDictionary(requiredValues),
                order,
                metadataCollection,
                null);
        }

        private class RouteNameMetadata : IRouteNameMetadata
        {
            public RouteNameMetadata(string name)
            {
                Name = name;
            }
            public string Name { get; }
        }

        private class NameMetadata : INameMetadata
        {
            public NameMetadata(string name)
            {
                Name = name;
            }
            public string Name { get; }
        }

        private class CustomRouteValuesBasedEndpointFinder : RouteValuesBasedEndpointFinder
        {
            public CustomRouteValuesBasedEndpointFinder(
                CompositeEndpointDataSource endpointDataSource,
                ObjectPool<UriBuildingContext> objectPool)
                : base(endpointDataSource, objectPool)
            {
            }

            public IEnumerable<OutboundMatch> AllMatches { get; private set; }

            public IDictionary<string, List<OutboundMatchResult>> NamedMatches { get; private set; }

            protected override (IEnumerable<OutboundMatch>, IDictionary<string, List<OutboundMatchResult>>) GetOutboundMatches()
            {
                var matches = base.GetOutboundMatches();
                AllMatches = matches.Item1;
                NamedMatches = matches.Item2;
                return matches;
            }
        }

        private class SuppressLinkGenerationMetadata : ISuppressLinkGenerationMetadata { }
    }
}
