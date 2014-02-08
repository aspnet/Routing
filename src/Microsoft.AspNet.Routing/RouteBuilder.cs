// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.AspNet.Routing
{
    public class RouteBuilder : IRouteBuilder
    {
        public RouteBuilder(IRouteCollection routes, IRouteEndpoint endpoint)
        {
            Routes = routes;
            Endpoint = endpoint;
        }

        public IRouteEndpoint Endpoint
        {
            get;
            private set;
        }

        public IRouteCollection Routes
        {
            get;
            private set;
        }
    }
}
