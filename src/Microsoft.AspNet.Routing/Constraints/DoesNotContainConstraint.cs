// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNet.Routing.Constraints
{
    /// <summary>
    /// Constrains a route parameter to not contain any of the input strings.
    /// </summary>
    public class DoesNotContainConstraint : IRouteConstraint
    {
        private readonly string[] m_ExcludedStrings;

        /// <summary>
        /// Initializes a new instance of the <see cref="DoesNotContainConstraint" /> class.
        /// </summary>
        public DoesNotContainConstraint(params string[] excludedStrings)
        {
            m_ExcludedStrings = excludedStrings;
        }

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
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

            object routeValue;

            if (values.TryGetValue(routeKey, out routeValue) && routeValue != null)
            {
                string parameterValueString = Convert.ToString(routeValue, CultureInfo.InvariantCulture);

                return m_ExcludedStrings.All(s => !parameterValueString.Contains(s));
            }

            return true;
        }
    }
}
