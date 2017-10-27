// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Dispatcher
{
    /// <summary>
    /// Defines a constraint on an optional parameter. If the parameter is present, then it is constrained by InnerConstraint. 
    /// </summary>
    public class OptionalRouteConstraint : IDispatcherValueConstraint
    {
        public OptionalRouteConstraint(IDispatcherValueConstraint innerConstraint)
        {
            if (innerConstraint == null)
            {
                throw new ArgumentNullException(nameof(innerConstraint));
            }

            InnerConstraint = innerConstraint;
        }

        public IDispatcherValueConstraint InnerConstraint { get; }

        public bool Match(DispatcherValueConstraintContext constraintContext)
        {
            if (constraintContext == null)
            {
                throw new ArgumentNullException(nameof(constraintContext));
            }

            if (constraintContext.Values.TryGetValue(constraintContext.Key, out var routeValue)
                && routeValue != null)
            {
                return InnerConstraint.Match(constraintContext);
            }

            return true;
        }
    }
}