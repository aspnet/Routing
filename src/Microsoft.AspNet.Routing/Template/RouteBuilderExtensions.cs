// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Routing.Template
{
    public static class RouteBuilderExtensions
    {
        public static IRouteBuilder AddTemplateRoute(this IRouteBuilder builder, string template)
        {
            return builder.AddTemplateRoute(template, null);
        }

        public static IRouteBuilder AddTemplateRoute(this IRouteBuilder builder, string template, object defaults)
        {
            builder.Routes.Add(new TemplateRoute(builder.Endpoint, template, null /* route value dictionary doesn't exist yet */));
            return builder;
        }
    }
}
