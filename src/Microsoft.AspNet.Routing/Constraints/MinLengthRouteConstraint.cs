// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Routing.Constraints
{
    /// <summary>
    /// Constrains a route parameter to be a string with a minimum length.
    /// </summary>
    public class MinLengthRouteConstraint : IRouteConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MinLengthRouteConstraint" /> class.
        /// </summary>
        /// <param name="minLength">The minimum length allowed for the route parameter.</param>
        public MinLengthRouteConstraint(int minLength)
        {
            if (minLength < 0)
            {
                var errorMessage = Resources.FormatArgumentMustBeGreaterThanOrEqualTo(0);
                throw new ArgumentOutOfRangeException("minLength", minLength, errorMessage);
            }

            MinLength = minLength;
        }

        /// <summary>
        /// Gets the minimum length of the route parameter.
        /// </summary>
        public int MinLength { get; private set; }

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
                var valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                return valueString.Length >= MinLength;
            }

            return false;
        }
    }
}