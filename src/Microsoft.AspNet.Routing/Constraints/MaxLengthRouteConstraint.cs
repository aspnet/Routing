// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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
        public MaxLengthRouteConstraint(int maxLength)
        {
            if (maxLength < 0)
            {
                var errorMessage = Resources.FormatArgumentMustBeGreaterThanOrEqualTo(0);
                throw new ArgumentOutOfRangeException("maxLength", maxLength, errorMessage);
            }

            MaxLength = maxLength;
        }

        /// <summary>
        /// Gets the maximum length of the route parameter.
        /// </summary>
        public int MaxLength { get; private set; }

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
                return valueString.Length <= MaxLength;
            }

            return false;
        }
    }
}