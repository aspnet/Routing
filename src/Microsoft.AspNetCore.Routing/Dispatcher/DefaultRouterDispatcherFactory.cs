// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Dispatcher;

namespace Microsoft.AspNetCore.Routing.Dispatcher
{
    internal class DefaultRouterDispatcherFactory : RouterDispatcherFactory
    {
        private readonly DispatcherBaseServices _services;
        private readonly IInlineConstraintResolver _constraintResolver;

        public DefaultRouterDispatcherFactory(DispatcherBaseServices services, IInlineConstraintResolver constraintResolver)
        {
            _services = services;
            _constraintResolver = constraintResolver;
        }

        public override DispatcherBase CreateDispatcher(string routeTemplate, RouteValuesEndpoint endpoint, params RouteValuesEndpoint[] additionalEndpoints)
        {
            if (routeTemplate == null)
            {
                throw new ArgumentNullException(nameof(routeTemplate));
            }

            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            if (additionalEndpoints == null)
            {
                throw new ArgumentNullException(nameof(additionalEndpoints));
            }

            var endpoints = new RouteValuesEndpoint[1 + additionalEndpoints.Length];
            endpoints[0] = endpoint;
            additionalEndpoints.CopyTo(endpoints, 1);

            var route = new Route(new RouterEndpointSelector(endpoints), routeTemplate, _constraintResolver);
            return new RouterDispatcher(_services, route);
        }
    }
}
