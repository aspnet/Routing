﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Routing;

namespace Microsoft.Framework.DependencyInjection
{
    /// <summary>
    /// Contains extension methods to IServiceCollection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures a set of <see cref="RouteOptions"/> for the application.
        /// </summary>
        /// <param name="services">The services available in the application.</param>
        /// <param name="setupAction">The <see cref="RouteOptions"/> which need to be configured.</param>
        public static void ConfigureRouteOptions(
            this IServiceCollection services,
            [NotNull] Action<RouteOptions> setupAction)
        {
            services.Configure(setupAction);
        }
    }
}