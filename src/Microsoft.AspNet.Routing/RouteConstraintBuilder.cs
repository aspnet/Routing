// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Routing.Constraints;
using Microsoft.AspNet.Routing.Template;

namespace Microsoft.AspNet.Routing
{
    public class RouteConstraintBuilder
    {
        private readonly IInlineConstraintResolver _inlineConstraintResolver;
        private readonly string _template;

        private readonly Dictionary<string, List<IRouteConstraint>> _constraints;

        public RouteConstraintBuilder(
            [NotNull] IInlineConstraintResolver inlineConstraintResolver,
            [NotNull] string template)
        {
            _inlineConstraintResolver = inlineConstraintResolver;
            _template = template;

            _constraints = new Dictionary<string, List<IRouteConstraint>>(StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyDictionary<string, IRouteConstraint> Build()
        {
            var constraints = new Dictionary<string, IRouteConstraint>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in _constraints)
            {
                IRouteConstraint constraint;
                if (kvp.Value.Count == 1)
                {
                    constraint = kvp.Value[0];
                }
                else
                {
                    constraint = new CompositeRouteConstraint(kvp.Value.ToArray());
                }

                constraints.Add(kvp.Key, constraint);
            }

            return constraints;
        }

        public void AddConstraint([NotNull] string key, [NotNull] object value)
        {
            var constraint = value as IRouteConstraint;
            if (constraint == null)
            {
                var regexPattern = value as string;
                if (regexPattern == null)
                {
                    throw new InvalidOperationException(
                        Resources.FormatRouteConstraintBuilder_ValidationMustBeStringOrCustomConstraint(
                            key,
                            value,
                            _template,
                            typeof(IRouteConstraint)));
                }

                var constraintsRegEx = "^(" + regexPattern + ")$";
                constraint = new RegexRouteConstraint(constraintsRegEx);
            }

            Add(key, constraint);
        }

        public void AddResolvedConstraint([NotNull] string key, [NotNull] string constraintText)
        {
            var constraint = _inlineConstraintResolver.ResolveConstraint(constraintText);
            if (constraint == null)
            {
                throw new InvalidOperationException(
                    Resources.FormatRouteConstraintBuilder_CouldNotResolveConstraint(
                        key,
                        constraintText,
                        _template,
                        _inlineConstraintResolver.GetType().Name));
            }

            Add(key, constraint);
        }

        private void Add(string key, IRouteConstraint constraint)
        {
            List<IRouteConstraint> list;
            if (!_constraints.TryGetValue(key, out list))
            {
                list = new List<IRouteConstraint>();
                _constraints.Add(key, list);
            }

            list.Add(constraint);
        }
    }
}
