// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
        public DefaultInlineConstraintResolver(IOptions<RouteOptions> routeOptions)
            : this(routeOptions, null)
        {
        }

        public DefaultInlineConstraintResolver(IOptions<RouteOptions> routeOptions, IServiceProvider serviceProvider)
        {
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

            string constraintKey;
            string argumentString;
            var indexOfFirstOpenParens = inlineConstraint.IndexOf('(');
            if (indexOfFirstOpenParens >= 0 && inlineConstraint.EndsWith(")", StringComparison.Ordinal))
            {
                constraintKey = inlineConstraint.Substring(0, indexOfFirstOpenParens);
                argumentString = inlineConstraint.Substring(indexOfFirstOpenParens + 1,
                                                            inlineConstraint.Length - indexOfFirstOpenParens - 2);
            }
            else
            {
                constraintKey = inlineConstraint;
                argumentString = null;
            }

            Type constraintType;
            if (!_inlineConstraintMap.TryGetValue(constraintKey, out constraintType))
            {
                // Cannot resolve the constraint key
                return null;
            }

            if (!typeof(IRouteConstraint).GetTypeInfo().IsAssignableFrom(constraintType.GetTypeInfo()))
            {
                throw new RouteCreationException(
                            Resources.FormatDefaultInlineConstraintResolver_TypeNotConstraint(
                                                        constraintType, constraintKey, typeof(IRouteConstraint).Name));
            }

            try
            {
                return CreateConstraint(constraintType, argumentString);
            }
            catch (RouteCreationException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new RouteCreationException(
                    $"An error occurred while trying to create an instance of route constraint '{constraintType.FullName}'.",
                    exception);
            }
        }

        internal IRouteConstraint CreateConstraint(Type constraintType, string argumentString)
        {
            // No arguments - call the default constructor
            if (argumentString == null)
            {
                return (IRouteConstraint)Activator.CreateInstance(constraintType);
            }

            var constraintTypeInfo = constraintType.GetTypeInfo();
            ConstructorInfo activationConstructor = null;
            object[] parameters = null;
            var constructors = constraintTypeInfo.DeclaredConstructors.ToArray();

            // If there is only one constructor and it has a single parameter, pass the argument string directly
            // This is necessary for the Regex RouteConstraint to ensure that patterns are not split on commas.
            if (constructors.Length == 1 && GetNonConvertableParameterTypeCount(constructors[0].GetParameters()) == 1)
            {
                activationConstructor = constructors[0];
                parameters = ConvertArguments(activationConstructor.GetParameters(), new string[] { argumentString });
            }
            else
            {
                var arguments = argumentString.Split(',').Select(argument => argument.Trim()).ToArray();

                var matchingConstructors = constructors.Where(ci => GetNonConvertableParameterTypeCount(ci.GetParameters()) == arguments.Length)
                                                       .ToArray();
                var constructorMatches = matchingConstructors.Length;

                if (constructorMatches == 0)
                {
                    throw new RouteCreationException(
                                Resources.FormatDefaultInlineConstraintResolver_CouldNotFindCtor(
                                                       constraintTypeInfo.Name, arguments.Length));
                }
                else if (constructorMatches == 1)
                {
                    activationConstructor = matchingConstructors[0];
                    parameters = ConvertArguments(activationConstructor.GetParameters(), arguments);
                }
                else
                {
                    throw new RouteCreationException(
                                Resources.FormatDefaultInlineConstraintResolver_AmbiguousCtors(
                                                       constraintTypeInfo.Name, arguments.Length));
                }
            }

            return (IRouteConstraint)activationConstructor.Invoke(parameters);
        }

        private int GetNonConvertableParameterTypeCount(ParameterInfo[] parameters)
        {
            if (_serviceProvider == null)
            {
                return parameters.Length;
            }

            var count = 0;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType.IsAssignableFrom(typeof(IConvertible)))
                {
                    count++;
                }
            }

            return count;
        }

        private object[] ConvertArguments(ParameterInfo[] parameterInfos, string[] arguments)
        {
            var parameters = new object[parameterInfos.Length];
            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var parameter = parameterInfos[i];
                var parameterType = parameter.ParameterType;

                if (_serviceProvider != null && !parameterType.IsAssignableFrom(typeof(IConvertible)))
                {
                    parameters[i] = _serviceProvider.GetService(parameterType);
                }
                else
                {
                    parameters[i] = Convert.ChangeType(arguments[i], parameterType, CultureInfo.InvariantCulture);
                }
            }

            return parameters;
        }
    }
}
