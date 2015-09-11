// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Routing.Constraints
{
    /// <summary>
    /// Defines a constraint on an optional parameter. If the parameter is present, then it is constrained by InnerConstraint. 
    /// </summary>
    public class OptionalRouteConstraint : IRouteConstraint
    {
        public OptionalRouteConstraint(IRouteConstraint innerConstraint)
        {
            if (innerConstraint == null)
            {
                throw new ArgumentNullException(nameof(innerConstraint));
            }

            InnerConstraint = innerConstraint;
        }

        public IRouteConstraint InnerConstraint { get; }

        public bool Match(HttpContext httpContext,
                          IRouter route,
                          string routeKey,
                          IDictionary<string, object> values,
                          RouteDirection routeDirection)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (route == null)
            {
                throw new ArgumentNullException(nameof(route));
            }

            if (routeKey == null)
            {
                throw new ArgumentNullException(nameof(routeKey));
            }

            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            object value;
            if (values.TryGetValue(routeKey, out value))
            {
                return InnerConstraint.Match(httpContext,
                                             route,
                                             routeKey,
                                             values,
                                             routeDirection);
            }

            return true;
        }
    }
}