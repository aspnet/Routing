using System;
using Microsoft.AspNet.Routing;

namespace RoutingSample.Web
{
    public static class RouteCollectionBuilderExtensions
    {
        public static IRouteCollectionBuilder AddPrefixRoute(this IRouteCollectionBuilder routeBuilder,
                                                             string prefix)
        {
            if (routeBuilder.DefaultHandler == null)
            {
                throw new InvalidOperationException("DefaultHandler must be set.");
            }

            if (routeBuilder.ServiceProvider == null)
            {
                throw new InvalidOperationException("ServiceProvider must be set.");
            }

            return AddPrefixRoute(routeBuilder, prefix, routeBuilder.DefaultHandler);
        }

        public static IRouteCollectionBuilder AddPrefixRoute(this IRouteCollectionBuilder routeBuilder,
                                                             string prefix,
                                                             IRouter handler)
        {
            routeBuilder.Routes.Add(new PrefixRoute(handler, prefix));
            return routeBuilder;
        }
    }
}