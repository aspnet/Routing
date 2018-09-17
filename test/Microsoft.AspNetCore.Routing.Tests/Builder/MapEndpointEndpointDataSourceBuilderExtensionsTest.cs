// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.Routing.Patterns;
using Xunit;

namespace Microsoft.AspNetCore.Builder
{
    public class MapEndpointEndpointDataSourceBuilderExtensionsTest
    {
        private BuilderEndpointDataSource GetBuilderEndpointDataSource(DefaultEndpointDataSourcesBuilder dataSourcesBuilder)
        {
            return Assert.IsType<BuilderEndpointDataSource>(Assert.Single(dataSourcesBuilder.EndpointDataSources));
        }

        private RouteEndpointBuilder GetRouteEndpointBuilder(DefaultEndpointDataSourcesBuilder dataSourcesBuilder)
        {
            return Assert.IsType<RouteEndpointBuilder>(Assert.Single(GetBuilderEndpointDataSource(dataSourcesBuilder).EndpointBuilders));
        }

        [Fact]
        public void MapEndpoint_StringPattern_BuildsEndpoint()
        {
            // Arrange
            var builder = new DefaultEndpointDataSourcesBuilder();
            RequestDelegate requestDelegate = (d) => null;

            // Act
            var endpointBuilder = builder.MapEndpoint(requestDelegate, "/", "Display name!");

            // Assert
            var endpointBuilder1 = GetRouteEndpointBuilder(builder);

            Assert.Equal(requestDelegate, endpointBuilder1.RequestDelegate);
            Assert.Equal("Display name!", endpointBuilder1.DisplayName);
            Assert.Equal("/", endpointBuilder1.RoutePattern.RawText);
        }

        [Fact]
        public void MapEndpoint_TypedPattern_BuildsEndpoint()
        {
            // Arrange
            var builder = new DefaultEndpointDataSourcesBuilder();
            RequestDelegate requestDelegate = (d) => null;

            // Act
            var endpointBuilder = builder.MapEndpoint(requestDelegate, RoutePatternFactory.Parse("/"), "Display name!");

            // Assert
            var endpointBuilder1 = GetRouteEndpointBuilder(builder);

            Assert.Equal(requestDelegate, endpointBuilder1.RequestDelegate);
            Assert.Equal("Display name!", endpointBuilder1.DisplayName);
            Assert.Equal("/", endpointBuilder1.RoutePattern.RawText);
        }

        [Fact]
        public void MapEndpoint_StringPatternAndMetadata_BuildsEndpoint()
        {
            // Arrange
            var metadata = new object();
            var builder = new DefaultEndpointDataSourcesBuilder();
            RequestDelegate requestDelegate = (d) => null;

            // Act
            var endpointBuilder = builder.MapEndpoint(requestDelegate, "/", "Display name!", new[] { metadata });

            // Assert
            var endpointBuilder1 = GetRouteEndpointBuilder(builder);
            Assert.Equal(requestDelegate, endpointBuilder1.RequestDelegate);
            Assert.Equal("Display name!", endpointBuilder1.DisplayName);
            Assert.Equal("/", endpointBuilder1.RoutePattern.RawText);
            Assert.Equal(metadata, Assert.Single(endpointBuilder1.Metadata));
        }

        [Fact]
        public void MapEndpoint_TypedPatternAndMetadata_BuildsEndpoint()
        {
            // Arrange
            var metadata = new object();
            var builder = new DefaultEndpointDataSourcesBuilder();
            RequestDelegate requestDelegate = (d) => null;

            // Act
            var endpointBuilder = builder.MapEndpoint(requestDelegate, RoutePatternFactory.Parse("/"), "Display name!", new[] { metadata });

            // Assert
            var endpointBuilder1 = GetRouteEndpointBuilder(builder);
            Assert.Equal(requestDelegate, endpointBuilder1.RequestDelegate);
            Assert.Equal("Display name!", endpointBuilder1.DisplayName);
            Assert.Equal("/", endpointBuilder1.RoutePattern.RawText);
            Assert.Equal(metadata, Assert.Single(endpointBuilder1.Metadata));
        }
    }
}
