﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing.Metadata;
using Microsoft.AspNetCore.Routing.Patterns;
using Xunit;
using static Microsoft.AspNetCore.Routing.Matchers.HttpMethodMatcherPolicy;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    public class HttpMethodMatcherPolicyTest
    {
        [Fact]
        public void AppliesToNode_EndpointWithoutMetadata_ReturnsFalse()
        {
            // Arrange
            var endpoints = new[] { CreateEndpoint("/", null), };

            var policy = CreatePolicy();

            // Act
            var result = policy.AppliesToNode(endpoints);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AppliesToNode_EndpointWithoutHttpMethods_ReturnsFalse()
        {
            // Arrange
            var endpoints = new[] 
            {
                CreateEndpoint("/", new HttpMethodMetadata(Array.Empty<string>())),
            };

            var policy = CreatePolicy();

            // Act
            var result = policy.AppliesToNode(endpoints);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void AppliesToNode_EndpointHasHttpMethods_ReturnsTrue()
        {
            // Arrange
            var endpoints = new[]
            {
                CreateEndpoint("/", new HttpMethodMetadata(Array.Empty<string>())),
                CreateEndpoint("/", new HttpMethodMetadata(new[] { "GET", })),
            };

            var policy = CreatePolicy();

            // Act
            var result = policy.AppliesToNode(endpoints);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetEdges_GroupsByHttpMethod()
        {
            // Arrange
            var endpoints = new[]
            {
                // These are arrange in an order that we won't actually see in a product scenario. It's done
                // this way so we can verify that ordering is preserved by GetEdges.
                CreateEndpoint("/", new HttpMethodMetadata(new[] { "GET", })),
                CreateEndpoint("/", new HttpMethodMetadata(Array.Empty<string>())),
                CreateEndpoint("/", new HttpMethodMetadata(new[] { "GET", "PUT", "POST" })),
                CreateEndpoint("/", new HttpMethodMetadata(new[] { "PUT", "POST" })),
                CreateEndpoint("/", new HttpMethodMetadata(Array.Empty<string>())),
            };

            var policy = CreatePolicy();

            // Act
            var edges = policy.GetEdges(endpoints);

            // Assert
            Assert.Collection(
                edges.OrderBy(e => e.State),
                e =>
                {
                    Assert.Equal(new EdgeKey(AnyMethod, isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("GET", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[0], endpoints[1], endpoints[2], endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("POST", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[2], endpoints[3], endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("PUT", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[2], endpoints[3], endpoints[4], }, e.Endpoints.ToArray());
                });
        }

        [Fact]
        public void GetEdges_GroupsByHttpMethod_Cors()
        {
            // Arrange
            var endpoints = new[]
            {
                // These are arrange in an order that we won't actually see in a product scenario. It's done
                // this way so we can verify that ordering is preserved by GetEdges.
                CreateEndpoint("/", new HttpMethodMetadata(new[] { "GET", })),
                CreateEndpoint("/", new HttpMethodMetadata(Array.Empty<string>())),
                CreateEndpoint("/", new HttpMethodMetadata(new[] { "GET", "PUT", "POST" }, acceptCorsPreflight: true)),
                CreateEndpoint("/", new HttpMethodMetadata(new[] { "PUT", "POST" })),
                CreateEndpoint("/", new HttpMethodMetadata(Array.Empty<string>(), acceptCorsPreflight: true)),
            };

            var policy = CreatePolicy();

            // Act
            var edges = policy.GetEdges(endpoints);

            // Assert
            Assert.Collection(
                edges.OrderBy(e => e.State),
                e =>
                {
                    Assert.Equal(new EdgeKey(AnyMethod, isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey(AnyMethod, isCorsPreflightRequest: true), e.State);
                    Assert.Equal(new[] { endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("GET", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[0], endpoints[1], endpoints[2], endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("GET", isCorsPreflightRequest: true), e.State);
                    Assert.Equal(new[] { endpoints[2], endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("POST", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[2], endpoints[3], endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("POST", isCorsPreflightRequest: true), e.State);
                    Assert.Equal(new[] { endpoints[2], endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("PUT", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[2], endpoints[3], endpoints[4], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("PUT", isCorsPreflightRequest: true), e.State);
                    Assert.Equal(new[] { endpoints[2], endpoints[4], }, e.Endpoints.ToArray());
                });
        }

        [Fact] // See explanation in GetEdges for how this case is different
        public void GetEdges_GroupsByHttpMethod_CreatesHttp405Endpoint()
        {
            // Arrange
            var endpoints = new[]
            {
                // These are arrange in an order that we won't actually see in a product scenario. It's done
                // this way so we can verify that ordering is preserved by GetEdges.
                CreateEndpoint("/", new HttpMethodMetadata(new[] { "GET", })),
                CreateEndpoint("/", new HttpMethodMetadata(new[] { "GET", "PUT", "POST" })),
                CreateEndpoint("/", new HttpMethodMetadata(new[] { "PUT", "POST" })),
            };

            var policy = CreatePolicy();

            // Act
            var edges = policy.GetEdges(endpoints);

            // Assert
            Assert.Collection(
                edges.OrderBy(e => e.State),
                e =>
                {
                    Assert.Equal(new EdgeKey(AnyMethod, isCorsPreflightRequest: false), e.State);
                    Assert.Equal(Http405EndpointDisplayName, e.Endpoints.Single().DisplayName);
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("GET", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[0], endpoints[1], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("POST", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[2], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("PUT", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[2], }, e.Endpoints.ToArray());
                });

        }

        [Fact] // See explanation in GetEdges for how this case is different
        public void GetEdges_GroupsByHttpMethod_CreatesHttp405Endpoint_CORS()
        {
            // Arrange
            var endpoints = new[]
            {
                // These are arrange in an order that we won't actually see in a product scenario. It's done
                // this way so we can verify that ordering is preserved by GetEdges.
                CreateEndpoint("/", new HttpMethodMetadata(new[] { "GET", })),
                CreateEndpoint("/", new HttpMethodMetadata(new[] { "GET", "PUT", "POST" }, acceptCorsPreflight: true)),
                CreateEndpoint("/", new HttpMethodMetadata(new[] { "PUT", "POST" })),
            };

            var policy = CreatePolicy();

            // Act
            var edges = policy.GetEdges(endpoints);

            // Assert
            Assert.Collection(
                edges.OrderBy(e => e.State),
                e =>
                {
                    Assert.Equal(new EdgeKey(AnyMethod, isCorsPreflightRequest: false), e.State);
                    Assert.Equal(Http405EndpointDisplayName, e.Endpoints.Single().DisplayName);
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("GET", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[0], endpoints[1], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("GET", isCorsPreflightRequest: true), e.State);
                    Assert.Equal(new[] { endpoints[1], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("POST", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[2], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("POST", isCorsPreflightRequest: true), e.State);
                    Assert.Equal(new[] { endpoints[1], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("PUT", isCorsPreflightRequest: false), e.State);
                    Assert.Equal(new[] { endpoints[1], endpoints[2], }, e.Endpoints.ToArray());
                },
                e =>
                {
                    Assert.Equal(new EdgeKey("PUT", isCorsPreflightRequest: true), e.State);
                    Assert.Equal(new[] { endpoints[1], }, e.Endpoints.ToArray());
                });
        }

        private static MatcherEndpoint CreateEndpoint(string template, HttpMethodMetadata httpMethodMetadata)
        {
            var metadata = new List<object>();
            if (httpMethodMetadata != null)
            {
                metadata.Add(httpMethodMetadata);
            }

            return new MatcherEndpoint(
                MatcherEndpoint.EmptyInvoker,
                RoutePatternFactory.Parse(template),
                new RouteValueDictionary(),
                0,
                new EndpointMetadataCollection(metadata),
                $"test: {template}");
        }

        private static HttpMethodMatcherPolicy CreatePolicy()
        {
            return new HttpMethodMatcherPolicy();
        }
    }
}
