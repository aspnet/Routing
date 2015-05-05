// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Routing.Constraints
{
    /// <summary>
    /// Constrains a route parameter to be a string of a given length or within a given range of lengths.
    /// </summary>
    public class LengthRouteConstraint : IRouteConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LengthRouteConstraint" /> class that constrains
        /// a route parameter to be a string of a given length.
        /// </summary>
        /// <param name="length">The length of the route parameter.</param>
        public LengthRouteConstraint(int length)
        {
            if (length < 0)
            {
                var errorMessage = Resources.FormatArgumentMustBeGreaterThanOrEqualTo(0);
                throw new ArgumentOutOfRangeException(nameof(length), errorMessage);
            }

            MinLength = MaxLength = length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LengthRouteConstraint" /> class that constrains
        /// a route parameter to be a string of a given length.
        /// </summary>
        /// <param name="minLength">The minimum length allowed for the route parameter.</param>
        /// <param name="maxLength">The maximum length allowed for the route parameter.</param>
        public LengthRouteConstraint(int minLength, int maxLength)
        {
            if (minLength < 0)
            {
                var errorMessage = Resources.FormatArgumentMustBeGreaterThanOrEqualTo(0);
                throw new ArgumentOutOfRangeException(nameof(minLength), errorMessage);
            }

            if (maxLength < 0)
            {
                var errorMessage = Resources.FormatArgumentMustBeGreaterThanOrEqualTo(0);
                throw new ArgumentOutOfRangeException(nameof(maxLength), errorMessage);
            }

            if (minLength > maxLength)
            {
                var errorMessage =
                    Resources.FormatRangeConstraint_MinShouldBeLessThanOrEqualToMax("minLength", "maxLength");
                throw new ArgumentOutOfRangeException(nameof(minLength), errorMessage);
            }

            MinLength = minLength;
            MaxLength = maxLength;
        }

        /// <summary>
        /// Gets the minimum length allowed for the route parameter.
        /// </summary>
        public int MinLength { get; private set; }

        /// <summary>
        /// Gets the maximum length allowed for the route parameter.
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
                var length = valueString.Length;
                return length >= MinLength && length <= MaxLength;
            }

            return false;
        }
    }
}