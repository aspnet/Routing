﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
            var router = builder.UseRouter();

            router
                .For(async (context) => await context.Response.WriteAsync("match1"))
                .AddTemplateRoute("api/{controller}", new { controller = "Home" })
                .AddPrefixRoute("hello/");

            router
                .For(async (context) => await context.Response.WriteAsync("Hello, World!"))
                .AddTemplateRoute("api/checkout/{*extra}")
                .AddPrefixRoute("");

            // Imagine something like this exists - mvc.Routes returns an IRouteBuilder
            var mvc = builder.UseMvc();
            mvc.Routes
                .AddRoute("{controller}/{action}/{id}");
        }
    }
}

#endif
