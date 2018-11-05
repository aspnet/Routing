﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;

namespace RoutingWebSite
{
    public class UseRouterStartup
    {
        private static readonly TimeSpan RegexMatchTimeout = TimeSpan.FromSeconds(10);

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouter(routes =>
            {
                routes.DefaultHandler = new RouteHandler((httpContext) =>
                {
                    var request = httpContext.Request;
                    return httpContext.Response.WriteAsync($"Verb =  {request.Method.ToUpperInvariant()} - Path = {request.Path} - Route values - {string.Join(", ", httpContext.GetRouteData().Values)}");
                });

                routes.MapGet("api/get/{id}", (request, response, routeData) => response.WriteAsync($"API Get {routeData.Values["id"]}"))
                      .MapMiddlewareRoute("api/middleware", (appBuilder) => appBuilder.Use((httpContext, next) => httpContext.Response.WriteAsync("Middleware!")))
                      .MapRoute(
                        name: "AllVerbs",
                        template: "api/all/{name}/{lastName?}",
                        defaults: new { lastName = "Doe" },
                        constraints: new { lastName = new RegexRouteConstraint(new Regex("[a-zA-Z]{3}", RegexOptions.CultureInvariant, RegexMatchTimeout)) });
            });

            app.Map("/Branch1", branch => SetupBranch(branch, "Branch1"));
            app.Map("/Branch2", branch => SetupBranch(branch, "Branch2"));
        }

        private void SetupBranch(IApplicationBuilder app, string name)
        {
            app.UseRouter(routes =>
            {
                routes.MapGet("api/get/{id}", (request, response, routeData) => response.WriteAsync($"{name} - API Get {routeData.Values["id"]}"));
            });
        }
    }
}
