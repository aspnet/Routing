﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Builder
{
    public static class RequestDelegateRouteBuilderExtensions
    {
        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> for the given <paramref name="template"/>, and
        /// <paramref name="handler"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The <see cref="RequestDelegate"/> route handler.</param>
        /// <returns>A reference to the <paramref name="builder"/> after this operation has completed.</returns>
        public static IRouteBuilder MapRoute(this IRouteBuilder builder, string template, RequestDelegate handler)
        {
            var route = new Route(
                new RouteHandler(handler),
                template,
                defaults: null,
                constraints: null,
                dataTokens: null,
                inlineConstraintResolver: GetConstraintResolver(builder));

            builder.Routes.Add(route);
            return builder;
        }

        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> for the given <paramref name="template"/>, and
        /// <paramref name="action"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/>.</param>
        /// <param name="action">The action to apply to the <see cref="IApplicationBuilder"/>.</param>
        /// <param name="handler">The <see cref="RequestDelegate"/> route handler.</param>
        /// <returns>A reference to the <paramref name="builder"/> after this operation has completed.</returns>
        public static IRouteBuilder MapRoute(this IRouteBuilder builder, string template, Action<IApplicationBuilder> action)
        {
            var nested = builder.ApplicationBuilder.New();
            action(nested);
            return builder.MapRoute(template, nested.Build());
        }

        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> that only matches HTTP DELETE requests for the given
        /// <paramref name="template"/>, and <paramref name="handler"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The <see cref="RequestDelegate"/> route handler.</param>
        /// <returns>A reference to the <paramref name="builder"/> after this operation has completed.</returns>
        public static IRouteBuilder MapDelete(this IRouteBuilder builder, string template, RequestDelegate handler)
        {
            return builder.MapVerb("DELETE", template, handler);
        }

        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> that only matches HTTP DELETE requests for the given
        /// <paramref name="template"/>, and <paramref name="action"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="action">The action to apply to the <see cref="IApplicationBuilder"/>.</param>
        /// <returns>A reference to the <paramref name="builder"/> after this operation has completed.</returns>
        public static IRouteBuilder MapDelete(this IRouteBuilder builder, string template, Action<IApplicationBuilder> action)
        {
            return builder.MapVerb("DELETE", template, action);
        }

        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> that only matches HTTP DELETE requests for the given
        /// <paramref name="template"/>, and <paramref name="handler"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The <see cref="RequestDelegate"/> route handler.</param>
        /// <returns>A reference to the <paramref name="builder"/> after this operation has completed.</returns>
        public static IRouteBuilder MapGet(this IRouteBuilder builder, string template, RequestDelegate handler)
        {
            return builder.MapVerb("GET", template, handler);
        }

        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> that only matches HTTP DELETE requests for the given
        /// <paramref name="template"/>, and <paramref name="action"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="action">The action to apply to the <see cref="IApplicationBuilder"/>.</param>
        /// <returns>A reference to the <paramref name="builder"/> after this operation has completed.</returns>
        public static IRouteBuilder MapGet(this IRouteBuilder builder, string template, Action<IApplicationBuilder> action)
        {
            return builder.MapVerb("GET", template, action);
        }

        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> that only matches HTTP DELETE requests for the given
        /// <paramref name="template"/>, and <paramref name="handler"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The <see cref="RequestDelegate"/> route handler.</param>
        /// <returns>A reference to the <paramref name="builder"/> after this operation has completed.</returns>
        public static IRouteBuilder MapPost(this IRouteBuilder builder, string template, RequestDelegate handler)
        {
            return builder.MapVerb("POST", template, handler);
        }

        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> that only matches HTTP DELETE requests for the given
        /// <paramref name="template"/>, and <paramref name="action"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="action">The action to apply to the <see cref="IApplicationBuilder"/>.</param>
        /// <returns>A reference to the <paramref name="builder"/> after this operation has completed.</returns>
        public static IRouteBuilder MapPost(this IRouteBuilder builder, string template, Action<IApplicationBuilder> action)
        {
            return builder.MapVerb("POST", template, action);
        }

        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> that only matches HTTP DELETE requests for the given
        /// <paramref name="template"/>, and <paramref name="handler"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The <see cref="RequestDelegate"/> route handler.</param>
        /// <returns>A reference to the <paramref name="builder"/> after this operation has completed.</returns>
        public static IRouteBuilder MapPut(this IRouteBuilder builder, string template, RequestDelegate handler)
        {
            return builder.MapVerb("PUT", template, handler);
        }

        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> that only matches HTTP DELETE requests for the given
        /// <paramref name="template"/>, and <paramref name="action"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="action">The action to apply to the <see cref="IApplicationBuilder"/>.</param>
        /// <returns>A reference to the <paramref name="builder"/> after this operation has completed.</returns>
        public static IRouteBuilder MapPut(this IRouteBuilder builder, string template, Action<IApplicationBuilder> action)
        {
            return builder.MapVerb("PUT", template, action);
        }

        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> that only matches HTTP requests for the given
        /// <paramref name="verb"/>, <paramref name="template"/>, and <paramref name="handler"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/>.</param>
        /// <param name="verb">The HTTP verb allowed by the route.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The <see cref="RequestDelegate"/> route handler.</param>
        /// <returns>A reference to the <paramref name="builder"/> after this operation has completed.</returns>
        public static IRouteBuilder MapVerb(
            this IRouteBuilder builder,
            string verb,
            string template,
            RequestDelegate handler)
        {
            var route = new Route(
                new RouteHandler(handler),
                template,
                defaults: null,
                constraints: new RouteValueDictionary(new { httpMethod = new HttpMethodRouteConstraint(verb) }),
                dataTokens: null,
                inlineConstraintResolver: GetConstraintResolver(builder));

            builder.Routes.Add(route);
            return builder;
        }

        /// <summary>
        /// Adds a route to the <see cref="IRouteBuilder"/> that only matches HTTP requests for the given
        /// <paramref name="verb"/>, <paramref name="template"/>, and <paramref name="action"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/>.</param>
        /// <param name="verb">The HTTP verb allowed by the route.</param>
        /// <param name="template">The route template.</param>
        /// <param name="action">The action to apply to the <see cref="IApplicationBuilder"/>.</param>
        /// <returns>A reference to the <paramref name="builder"/> after this operation has completed.</returns>
        public static IRouteBuilder MapVerb(
            this IRouteBuilder builder,
            string verb,
            string template,
            Action<IApplicationBuilder> action)
        {
            var nested = builder.ApplicationBuilder.New();
            action(nested);
            return builder.MapVerb(verb, template, nested.Build());
        }

        private static IInlineConstraintResolver GetConstraintResolver(IRouteBuilder builder)
        {
            return builder.ServiceProvider.GetRequiredService<IInlineConstraintResolver>();
        }
    }
}
