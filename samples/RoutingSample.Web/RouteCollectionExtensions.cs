using System;
using Microsoft.AspNet.Routing;

namespace RoutingSample.Web
{
    public static class RouteBuilderExtensions
    {
        public static IRouteCollectionBuilder AddPrefixRoute(this IRouteCollectionBuilder routeBuilder, string prefix)
        {
            if (routeBuilder.DefaultHandler == null)
            {
                throw new ArgumentException("DefaultHandler must be set.");
            }

            if (routeBuilder.ServiceProvider == null)
            {
                throw new ArgumentException("ServiceProvider must be set.");
            }

            return AddPrefixRoute(routeBuilder, prefix, routeBuilder.DefaultHandler);
        }

        public static IRouteCollectionBuilder AddPrefixRoute(this IRouteCollectionBuilder routeBuilder, string prefix, IRouter handler)
        {
            routeBuilder.Routes.Add(new PrefixRoute(handler, prefix));
            return routeBuilder;
        }
    }
}