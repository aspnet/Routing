// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System;
using System.Threading.Tasks;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Routing
{
    public static class RoutingServices
    {
        public static IServiceCollection AddRouting(this IServiceCollection services, IConfiguration config = null, Action<RouteOptions> configureOptions = null)
        {
            var describe = new ServiceDescriber(config);

            services.AddOptions(config);
            services.TryAdd(describe.Transient<IInlineConstraintResolver, DefaultInlineConstraintResolver>());

            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }

            return services;
        }
    }
}
