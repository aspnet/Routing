// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Routing.Constraints
{
    /// <summary>
    /// Constraints a route parameter to be an integer within a given range of values.
    /// </summary>
    public class RangeRouteConstraint : IRouteConstraint
    {
        public RangeRouteConstraint(long min, long max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Gets the minimum value of the route parameter.
        /// </summary>
        public long Min { get; private set; }

        /// <summary>
        /// Gets the maximum value of the route parameter.
        /// </summary>
        public long Max { get; private set; }

        /// <inheritdoc />
        public bool Match([NotNull] HttpContext httpContext, 
                          [NotNull] IRouter route,
                          [NotNull] string routeKey,
                          [NotNull] IDictionary<string, object> values,
                          RouteDirection routeDirection)
        {
            object value;
            if (values.TryGetValue(routeKey, out value) && value != null)
            {
                long longValue;
                if (value is long)
                {
                    longValue = (long)value;
                    return longValue >= Min && longValue <= Max;
                }

                var valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (Int64.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out longValue))
                {
                    return longValue >= Min && longValue <= Max;
                }
            }

            return false;
        }
    }
}