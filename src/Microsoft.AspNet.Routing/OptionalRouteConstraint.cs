// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Routing
{
    public class OptionalRouteConstraint : IRouteConstraint
    {
        public OptionalRouteConstraint([NotNull]IRouteConstraint innerConstraint)
        {
            InnerConstraint = innerConstraint;
        }

        public IRouteConstraint InnerConstraint { get; private set; }

        public bool Match([NotNull]HttpContext httpContext,
                          [NotNull]IRouter route,
                          [NotNull]string routeKey,
                          [NotNull]IDictionary<string, object> values,
                          RouteDirection routeDirection)
        {
            object value;
            if (values.TryGetValue(routeKey, out value) && value != null)
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