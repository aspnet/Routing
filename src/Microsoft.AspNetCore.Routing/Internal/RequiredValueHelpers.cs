// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Microsoft.AspNetCore.Routing.Internal
{
    internal static class RequiredValueHelpers
    {
        public static bool TryGetRequiredValue(RoutePattern routePattern, RoutePatternParameterPart parameterPart, out object value)
        {
            if (!routePattern.RequiredValues.TryGetValue(parameterPart.Name, out value))
            {
                return false;
            }

            return !RouteValueEqualityComparer.Default.Equals(value, string.Empty);
        }
    }
}
