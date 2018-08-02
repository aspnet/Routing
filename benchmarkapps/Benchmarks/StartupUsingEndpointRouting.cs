﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Benchmarks
{
    public class StartupUsingEndpointRouting
    {
        private static readonly byte[] _helloWorldPayload = Encoding.UTF8.GetBytes("Hello, World!");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();

            var endpointDataSource = new DefaultEndpointDataSource(new[]
                {
                    new MatcherEndpoint(
                        invoker: (next) => (httpContext) =>
                        {
                            var response = httpContext.Response;
                            var payloadLength = _helloWorldPayload.Length;
                            response.StatusCode = 200;
                            response.ContentType = "text/plain";
                            response.ContentLength = payloadLength;
                            return response.Body.WriteAsync(_helloWorldPayload, 0, payloadLength);
                        },
                        routePattern: RoutePatternFactory.Parse("/plaintext"),
                        order: 0,
                        metadata: EndpointMetadataCollection.Empty,
                        displayName: "Plaintext"),
                });

            services.TryAddEnumerable(ServiceDescriptor.Singleton<EndpointDataSource>(endpointDataSource));
        }

        public void Configure(Microsoft.AspNetCore.Builder.IApplicationBuilder app)
        {
            app.UseEndpointRouting();

            app.UseEndpoint();
        }
    }
}