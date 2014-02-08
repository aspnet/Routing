// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.AspNet.Abstractions;

namespace Microsoft.AspNet.Routing
{
    public static class RouteCollectionExtensions
    {
        public static IRouteBuilder For(this IRouteCollection routes, RequestDelegate action)
        {
            return new RouteBuilder(routes, new HttpContextRouteEndpoint(action));
        }
    }
}
