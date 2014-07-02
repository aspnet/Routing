﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Routing.Constraints
{
    /// <summary>
    /// Constraints a route parameter that must have a value.
    /// </summary>
    /// <remarks>
    /// This constraint is primarily used to enforce that a non-parameter value is present during 
    /// URL generation.
    /// </remarks>
    public class RequiredConstraint : IRouteConstraint
    {
        /// <inheritdoc />
        public bool Match(
            [NotNull]HttpContext httpContext, 
            [NotNull]IRouter route, 
            [NotNull]string routeKey, 
            [NotNull]IDictionary<string, object> values, 
            RouteDirection routeDirection)
        {
            object value;
            if (values.TryGetValue(routeKey, out value) && value != null)
            {
                // In routing the empty string is equivalent to null, which is equivalent to an unset value.
                var valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                return valueString.Length > 0;
            }

            return false;
        }
    }
}