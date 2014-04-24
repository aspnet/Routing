// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Routing
{
    public class RouteCollection : IRouteCollection
    {
        private readonly List<IRouter> _routes = new List<IRouter>();
        private readonly List<IRouter> _pureRoutes = new List<IRouter>();
        private readonly Dictionary<string, INamedRouter> _namedRoutes = new Dictionary<string, INamedRouter>();
        public IRouter this[int index]
        {
            get { return _routes[index]; }
        }

        public int Count
        {
            get { return _routes.Count; }
        }

        public IRouter DefaultHandler { get; set; }

        public void Add(IRouter router)
        {
            var namedRouter = router as INamedRouter;
            if (namedRouter != null)
            {
                if (!String.IsNullOrEmpty(namedRouter.Name))
                {
                    _namedRoutes.Add(namedRouter.Name, namedRouter);
                }
            }
            else
            {
                _pureRoutes.Add(router);
            }

            _routes.Add(router);
        }

        public async virtual Task RouteAsync(RouteContext context)
        {
            for (var i = 0; i < Count; i++)
            {
                var route = this[i];

                await route.RouteAsync(context);
                if (context.IsHandled)
                {
                    return;
                }
            }
        }

        public virtual string GetVirtualPath(VirtualPathContext context)
        {
            if (!String.IsNullOrEmpty(context.RouteName))
            {
                INamedRouter matchedNamedRoute;
                _namedRoutes.TryGetValue(context.RouteName, out matchedNamedRoute);
                IRouter matchedRoute = matchedNamedRoute;

                string virtualPath = null;
                foreach (var route in _pureRoutes)
                {
                    virtualPath = route.GetVirtualPath(context);
                    if (virtualPath != null)
                    {
                        if (matchedRoute != null)
                        {
                            // There was already a previous route which matched the name.
                            throw new InvalidOperationException(
                                                        Resources.
                                                        FormatNamedRoutes_AmbiguousRoutesFound(context.RouteName));
                        }

                        matchedRoute = route;
                    }
                }

                return virtualPath ?? matchedRoute.GetVirtualPath(context);
            }
            else
            {
                for (var i = 0; i < Count; i++)
                {
                    var route = this[i];

                    var path = route.GetVirtualPath(context);
                    if (path != null)
                    {
                        return path;
                    }
                }
            }

            return null;
        }
    }
}