// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Builder;

namespace Microsoft.AspNet.Routing
{
    public class RouteBuilder : IRouteBuilder
    {
        public RouteBuilder(IApplicationBuilder applicationBuilder)
            : this(applicationBuilder, defaultHandler: null)
        {
        }

        public RouteBuilder(IApplicationBuilder applicationBuilder, IRouter defaultHandler)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            ApplicationBuilder = applicationBuilder;
            ServiceProvider = applicationBuilder.ApplicationServices;

            Routes = new List<IRouter>();
        }

        public IApplicationBuilder ApplicationBuilder { get; }

        public IRouter DefaultHandler { get; set; }

        public IServiceProvider ServiceProvider { get; }

        public IList<IRouter> Routes { get; }

        public IRouter Build()
        {
            var routeCollection = new RouteCollection();

            foreach (var route in Routes)
            {
                routeCollection.Add(route);
            }

            return routeCollection;
        }
    }
}