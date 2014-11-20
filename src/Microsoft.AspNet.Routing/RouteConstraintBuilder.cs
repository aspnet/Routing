// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Routing.Constraints;

namespace Microsoft.AspNet.Routing
{
    /// <summary>
    /// A builder for produding a mapping of keys to see <see cref="IRouteConstraint"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="RouteConstraintBuilder"/> allows iterative building a set of route constraints, and will
    /// merge multiple entries for the same key.
    /// </remarks>
    public class RouteConstraintBuilder
    {
        private readonly IInlineConstraintResolver _inlineConstraintResolver;
        private readonly string _displayName;

        private readonly Dictionary<string, List<IRouteConstraint>> _constraints;
        private readonly HashSet<string> _optionalParameters;
        /// <summary>
        /// Creates a new <see cref="RouteConstraintBuilder"/> instance.
        /// </summary>
        /// <param name="inlineConstraintResolver">The <see cref="IInlineConstraintResolver"/>.</param>
        /// <param name="displayName">The display name (for use in error messages).</param>
        public RouteConstraintBuilder(
            [NotNull] IInlineConstraintResolver inlineConstraintResolver,
            [NotNull] string displayName)
        {
            _inlineConstraintResolver = inlineConstraintResolver;
            _displayName = displayName;

            _constraints = new Dictionary<string, List<IRouteConstraint>>(StringComparer.OrdinalIgnoreCase);
            _optionalParameters = new HashSet<string>();
        }

        /// <summary>
        /// Builds a mapping of constraints.
        /// </summary>
        /// <returns>An <see cref="IReadOnlyDictionary{string, IRouteConstraint}"/> of the constraints.</returns>
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

                if (_optionalParameters.Contains(kvp.Key))
                {
                    OptionalRouteConstraint opConstraint = new OptionalRouteConstraint(constraint);
                    constraints.Add(kvp.Key, opConstraint);
                }
                else
                {
                    constraints.Add(kvp.Key, constraint);
                }
            }

            return constraints;
        }

        /// <summary>
        /// Adds a constraint instance for the given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">
        /// The constraint instance. Must either be a string or an instance of <see cref="IRouteConstraint"/>.
        /// </param>
        /// <remarks>
        /// If the <paramref name="value"/> is a string, it will be converted to a <see cref="RegexRouteConstraint"/>.
        /// 
        /// For example, the string <code>Product[0-9]+</code> will be converted to the regular expression
        /// <code>^(Product[0-9]+)</code>. See <see cref="System.Text.RegularExpressions.Regex"/> for more details.
        /// </remarks>
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
                            _displayName,
                            typeof(IRouteConstraint)));
                }

                var constraintsRegEx = "^(" + regexPattern + ")$";
                constraint = new RegexRouteConstraint(constraintsRegEx);
            }

            Add(key, constraint);
        }

        /// <summary>
        /// Adds a constraint for the given key, resolved by the <see cref="IInlineConstraintResolver"/>.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="constraintText">The text to be resolved by <see cref="IInlineConstraintResolver"/>.</param>
        /// <remarks>
        /// The <see cref="IInlineConstraintResolver"/> can create <see cref="IRouteConstraint"/> instances
        /// based on <paramref name="constraintText"/>. See <see cref="RouteOptions.ConstraintMap"/> to register
        /// custom constraint types.
        /// </remarks>
        public void AddResolvedConstraint([NotNull] string key, [NotNull] string constraintText)
        {
            var constraint = _inlineConstraintResolver.ResolveConstraint(constraintText);
            if (constraint == null)
            {
                throw new InvalidOperationException(
                    Resources.FormatRouteConstraintBuilder_CouldNotResolveConstraint(
                        key,
                        constraintText,
                        _displayName,
                        _inlineConstraintResolver.GetType().Name));
            }

            Add(key, constraint);
        }
        public void SetOptional([NotNull] string name)
        {
            _optionalParameters.Add(name); 
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
