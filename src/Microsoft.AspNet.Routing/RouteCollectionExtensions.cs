// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Routing.Template;

namespace Microsoft.AspNet.Routing
{
    public static class RouteCollectionExtensions
    {
        public static IRouteCollection MapRoute(this IRouteCollection routes, string template)
        {
            MapRoute(routes, template, string.Empty);
            return routes;
        }

        public static IRouteCollection MapRoute(this IRouteCollection routes, string template, string name)
        {
            MapRoute(routes, template, name, defaults: null);
            return routes;
        }

        public static IRouteCollection MapRoute(this IRouteCollection routes, string template, string name,
                                                object defaults)
        {
            MapRoute(routes, template, name, new RouteValueDictionary(defaults));
            return routes;
        }

        public static IRouteCollection MapRoute(this IRouteCollection routes, string template, string name,
                                                IDictionary<string, object> defaults)
        {
            if (routes.DefaultHandler == null)
            {
                throw new InvalidOperationException(Resources.DefaultHandler_MustBeSet);
            }

            routes.Add(new TemplateRoute(routes.DefaultHandler, template, name, defaults, constraints: null));
            return routes;
        }

        public static IRouteCollection MapRoute(this IRouteCollection routes, string template,
                                                object defaults, object constraints)
        {
            MapRoute(routes,
                    template, 
                    string.Empty,
                    new RouteValueDictionary(defaults),
                    new RouteValueDictionary(constraints));

            return routes;
        }

        public static IRouteCollection MapRoute(this IRouteCollection routes, string template, string name,
                                            object defaults, object constraints)
        {
            MapRoute(routes, template, name, new RouteValueDictionary(defaults), new RouteValueDictionary(constraints));
            return routes;
        }

        public static IRouteCollection MapRoute(this IRouteCollection routes, string template, string name,
                                                object defaults, IDictionary<string, object> constraints)
        {
            MapRoute(routes, template, name, new RouteValueDictionary(defaults), constraints);
            return routes;
        }

        public static IRouteCollection MapRoute(this IRouteCollection routes, string template, string name,
                                                IDictionary<string, object> defaults, object constraints)
        {
            MapRoute(routes, template, name, defaults, new RouteValueDictionary(constraints));
            return routes;
        }

        public static IRouteCollection MapRoute(this IRouteCollection routes, string template, string name,
                                                IDictionary<string, object> defaults, IDictionary<string, object> constraints)
        {
            if (routes.DefaultHandler == null)
            {
                throw new InvalidOperationException(Resources.DefaultHandler_MustBeSet);
            }

            routes.Add(new TemplateRoute(routes.DefaultHandler, template, name, defaults, constraints));
            return routes;
        }
    }
}
