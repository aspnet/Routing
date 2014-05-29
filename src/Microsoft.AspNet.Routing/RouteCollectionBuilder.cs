// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Routing
{
    public class RouteCollectionBuilder : IRouteCollectionBuilder
    {
        public RouteCollectionBuilder([NotNull] IRouteCollection routeCollection)
        {
            Routes = routeCollection;
        }

        public IRouter DefaultHandler { get; set; }

        public IServiceProvider ServiceProvider { get; set; }

        public IRouteCollection Routes { get; private set; }
    }
}