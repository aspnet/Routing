﻿﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET45

using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Routing.Owin;
using Microsoft.AspNet.Routing.Template;
using Owin;

namespace RoutingSample
{
    internal class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            builder.UseErrorPage();

            builder.UseBuilder(ConfigureRoutes);
        }

        private void ConfigureRoutes(IBuilder builder)
        {            
            var routes = builder.UseRouter();

            var endpoint1 = new HttpContextRouteEndpoint(async (context) => await context.Response.WriteAsync("match1"));
            var endpoint2 = new HttpContextRouteEndpoint(async (context) => await context.Response.WriteAsync("Hello, World!"));

            var rb1 = new RouteBuilder(endpoint1, routes.Routes);
            rb1.AddPrefixRoute("api/store");
            rb1.AddTemplateRoute("api/checkout/{*extra}");

            var rb2 = new RouteBuilder(endpoint2, routes.Routes);
            rb2.AddPrefixRoute("hello/world");
            rb2.AddPrefixRoute("");
        }
    }
}

#endif
