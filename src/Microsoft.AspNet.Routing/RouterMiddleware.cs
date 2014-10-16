// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.RequestContainer;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Routing.Logging;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Builder
{
    public class RouterMiddleware : AutoRequestServicesMiddleware
    {
        private ILogger _logger;

        public RouterMiddleware(RequestDelegate next, IServiceProvider services, IRouter router) : base(next, services)
        {
            Router = router;
        }

        private IRouter Router
        {
            get;
            set;
        }

        public override async Task InvokeCore(HttpContext httpContext)
        {
            EnsureLogger(httpContext);
            using (_logger.BeginScope("RouterMiddleware.Invoke"))
            {
                var context = new RouteContext(httpContext);
                context.RouteData.Routers.Add(Router);

                await Router.RouteAsync(context);

                if (_logger.IsEnabled(TraceType.Information))
                {
                    _logger.WriteValues(new RouterMiddlewareInvokeValues() { Handled = context.IsHandled });
                }

                if (!context.IsHandled)
                {
                    await Next.Invoke(httpContext);
                }
            }
        }

        private void EnsureLogger(HttpContext context)
        {
            if (_logger == null)
            {
                var factory = context.RequestServices.GetService<ILoggerFactory>();
                _logger = factory.Create<RouterMiddleware>();
            }
        }
    }
}
