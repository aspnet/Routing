// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Dispatcher
{
    public static class DispatcherValueConstraintMatcher
    {
        public static bool Match(
            IDictionary<string, IDispatcherValueConstraint> constraints,
            DispatcherValueConstraintContext constraintContext,
            ILogger logger)
        {
            if (constraintContext == null)
            {
                throw new ArgumentNullException(nameof(constraintContext));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (constraints == null || constraints.Count == 0)
            {
                return true;
            }

            foreach (var kvp in constraints)
            {
                var constraint = kvp.Value;
                if (!constraint.Match(constraintContext))
                {
                    if (constraintContext.Purpose.Equals(ConstraintPurpose.IncomingRequest))
                    {
                        constraintContext.Values.TryGetValue(kvp.Key, out var routeValue);

                        logger.RouteValueDoesNotMatchConstraint(routeValue, kvp.Key, kvp.Value);
                    }

                    return false;
                }
            }

            return true;
        }
    }
}
