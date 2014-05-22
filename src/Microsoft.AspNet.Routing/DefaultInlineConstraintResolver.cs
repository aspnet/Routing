﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNet.Routing
{
    /// <summary>
    /// The default implementation of <see cref="IInlineConstraintResolver"/>. Resolves constraints by parsing
    /// a constraint key and constraint arguments, using a map to resolve the constraint type, and calling an
    /// appropriate constructor for the constraint type.
    /// </summary>
    public class DefaultInlineConstraintResolver : IInlineConstraintResolver
    {
        private readonly IDictionary<string, Type> _inlineConstraintMap = GetDefaultConstraintMap();

        /// <summary>
        /// Gets the mutable dictionary that maps constraint keys to a particular constraint type.
        /// </summary>
        public IDictionary<string, Type> ConstraintMap
        {
            get
            {
                return _inlineConstraintMap;
            }
        }

        private static IDictionary<string, Type> GetDefaultConstraintMap()
        {
            return new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                // Type-specific constraints
                { "int", typeof(IntRouteConstraint) },
            };
        }

        /// <inheritdoc />
        public virtual IRouteConstraint ResolveConstraint([NotNull] string inlineConstraint)
        {
            string constraintKey;
            string argumentString;
            int indexOfFirstOpenParens = inlineConstraint.IndexOf('(');
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
                throw new InvalidOperationException(
                            Resources.FormatDefaultInlineConstraintResolver_TypeNotConstraint(
                                                                constraintType, constraintKey));
            }

            return CreateConstraint(constraintType, argumentString);
        }

        private static IRouteConstraint CreateConstraint(Type constraintType, string argumentString)
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
            if (constructors.Length == 1 && constructors[0].GetParameters().Length == 1)
            {
                activationConstructor = constructors[0];
                parameters = ConvertArguments(activationConstructor.GetParameters(), new string[] { argumentString });
            }
            else
            {
                string[] arguments = argumentString.Split(',').Select(argument => argument.Trim()).ToArray();

                var matchingConstructors = constructors.Where(ci => ci.GetParameters().Length == arguments.Length).ToArray();
                var constructorMatches = matchingConstructors.Length;

                if (constructorMatches == 0)
                {
                    throw new InvalidOperationException(
                                Resources.FormatDefaultInlineConstraintResolver_CouldNotFindCtor(
                                                       constraintTypeInfo.Name, argumentString.Length));
                }
                else if (constructorMatches == 1)
                {
                    activationConstructor = matchingConstructors[0];
                    parameters = ConvertArguments(activationConstructor.GetParameters(), arguments);
                }
                else
                {
                    throw new InvalidOperationException(
                                Resources.FormatDefaultInlineConstraintResolver_AmbiguousCtors(
                                                       constraintTypeInfo.Name, argumentString.Length));
                }
            }

            return (IRouteConstraint)activationConstructor.Invoke(parameters);
        }

        private static object[] ConvertArguments(ParameterInfo[] parameterInfos, string[] arguments)
        {
            var parameters = new object[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                var parameter = parameterInfos[i];
                var parameterType = parameter.ParameterType;
                parameters[i] = Convert.ChangeType(arguments[i], parameterType, CultureInfo.InvariantCulture);
            }

            return parameters;
        }
    }
}
