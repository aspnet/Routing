// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Routing
{
    /// <summary>
    /// The default implementation of <see cref="IInlineConstraintResolver"/>. Resolves constraints by parsing
    /// a constraint key and constraint arguments, using a map to resolve the constraint type, and calling an
    /// appropriate constructor for the constraint type.
    /// </summary>
    public class DefaultInlineConstraintResolver : IInlineConstraintResolver
    {
        private readonly IDictionary<string, Type> _inlineConstraintMap;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultInlineConstraintResolver"/> class.
        /// </summary>
        /// <param name="routeOptions">
        /// Accessor for <see cref="RouteOptions"/> containing the constraints of interest.
        /// </param>
        [Obsolete("This constructor is obsolete. Use DefaultInlineConstraintResolver.ctor(IOptions<RouteOptions>, IServiceProvider) instead.")]
        public DefaultInlineConstraintResolver(IOptions<RouteOptions> routeOptions)
        {
            _inlineConstraintMap = routeOptions.Value.ConstraintMap;
        }

        public DefaultInlineConstraintResolver(IOptions<RouteOptions> routeOptions, IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            _inlineConstraintMap = routeOptions.Value.ConstraintMap;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        /// <example>
        /// A typical constraint looks like the following
        /// "exampleConstraint(arg1, arg2, 12)".
        /// Here if the type registered for exampleConstraint has a single constructor with one argument,
        /// The entire string "arg1, arg2, 12" will be treated as a single argument.
        /// In all other cases arguments are split at comma.
        /// </example>
        public virtual IRouteConstraint ResolveConstraint(string inlineConstraint)
        {
            if (inlineConstraint == null)
            {
                throw new ArgumentNullException(nameof(inlineConstraint));
            }

            return ParameterPolicyActivator.ResolveParameterPolicy<IRouteConstraint>(_inlineConstraintMap, _serviceProvider, inlineConstraint, out _);
        }
    }
}
