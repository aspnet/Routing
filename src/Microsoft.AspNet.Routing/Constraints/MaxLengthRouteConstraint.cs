// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Routing.Constraints
{
    /// <summary>
    /// Constrains a route parameter to be a string with a maximum length.
    /// </summary>
    public class MaxLengthRouteConstraint : IRouteConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaxLengthRouteConstraint" /> class.
        /// </summary>
        /// <param name="maxLength">The maximum length allowed for the route parameter.</param>
        public MaxLengthRouteConstraint(int maxLength)
        {
            if (maxLength < 0)
            {
                var errorMessage = Resources.FormatArgumentMustBeGreaterThanOrEqualTo(0);
                throw new ArgumentOutOfRangeException(nameof(maxLength), maxLength, errorMessage);
            }

            MaxLength = maxLength;
        }

        /// <summary>
        /// Gets the maximum length allowed for the route parameter.
        /// </summary>
        public int MaxLength { get; private set; }

        /// <inheritdoc />
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
            if (values.TryGetValue(routeKey, out value) && value != null)
            {
                var valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                return valueString.Length <= MaxLength;
            }

            return false;
        }
    }
}