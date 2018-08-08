// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Microsoft.AspNetCore.Builder
{
    public static class MapEndpointEndpointDataSourceBuilderExtensions
    {
        public static EndpointBuilder MapEndpoint(
            this EndpointDataSourceBuilder builder,
            Func<RequestDelegate, RequestDelegate> invoker,
            string name,
            string pattern)
        {
            return MapEndpoint(builder, invoker, name, pattern, null);
        }

        public static EndpointBuilder MapEndpoint(
            this EndpointDataSourceBuilder builder,
            Func<RequestDelegate, RequestDelegate> invoker,
            string name,
            RoutePattern pattern)
        {
            return MapEndpoint(builder, invoker, name, pattern, null);
        }

        public static EndpointBuilder MapEndpoint(
            this EndpointDataSourceBuilder builder,
            Func<RequestDelegate, RequestDelegate> invoker,
            string name,
            string template,
            IList<object> metadata)
        {
            return MapEndpoint(builder, invoker, name, RoutePatternFactory.Parse(template), metadata);
        }

        public static EndpointBuilder MapEndpoint(
            this EndpointDataSourceBuilder builder,
            Func<RequestDelegate, RequestDelegate> invoker,
            string name,
            RoutePattern pattern,
            IList<object> metadata)
        {
            const int defaultOrder = 0;

            var endpointBuilder = new MatcherEndpointBuilder(
               invoker,
               pattern,
               new RouteValueDictionary(),
               defaultOrder);
            endpointBuilder.DisplayName = name;
            if (metadata != null)
            {
                foreach (var item in metadata)
                {
                    endpointBuilder.Metadata.Add(item);
                }
            }

            builder.Endpoints.Add(endpointBuilder);
            return endpointBuilder;
        }
    }
}
