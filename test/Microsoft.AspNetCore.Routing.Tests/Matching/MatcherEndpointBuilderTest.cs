﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Patterns;
using Xunit;

namespace Microsoft.AspNetCore.Routing.Matching
{
    public class MatcherEndpointBuilderTest
    {
        [Fact]
        public void Build_AllValuesSet_EndpointCreated()
        {
            const int defaultOrder = 0;
            object metadata = new object();
            Func<RequestDelegate, RequestDelegate> invoker = (d) => null;

            var builder = new MatcherEndpointBuilder(invoker, RoutePatternFactory.Parse("/"), defaultOrder)
            {
                DisplayName = "Display name!",
                Metadata = { metadata }
            };

            var endpoint = Assert.IsType<MatcherEndpoint>(builder.Build());
            Assert.Equal("Display name!", endpoint.DisplayName);
            Assert.Equal(defaultOrder, endpoint.Order);
            Assert.Equal(invoker, endpoint.Invoker);
            Assert.Equal("/", endpoint.RoutePattern.RawText);
            Assert.Equal(metadata, Assert.Single(endpoint.Metadata));
        }
    }
}
