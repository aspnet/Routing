using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Routing.Internal
{
    internal static class ParameterPolicyActivator
    {
        public static T ResolveParameterPolicy<T>(IDictionary<string, Type> inlineParameterPolicyMap, IServiceProvider serviceProvider, string inlineParameterPolicy, out string parameterPolicyKey)
            where T : IParameterPolicy
        {
            if (inlineParameterPolicyMap == null)
            {
                throw new ArgumentNullException(nameof(inlineParameterPolicyMap));
            }

            if (inlineParameterPolicy == null)
            {
                throw new ArgumentNullException(nameof(inlineParameterPolicy));
            }

            string argumentString;
            var indexOfFirstOpenParens = inlineParameterPolicy.IndexOf('(');
            if (indexOfFirstOpenParens >= 0 && inlineParameterPolicy.EndsWith(")", StringComparison.Ordinal))
            {
                parameterPolicyKey = inlineParameterPolicy.Substring(0, indexOfFirstOpenParens);
                argumentString = inlineParameterPolicy.Substring(
                    indexOfFirstOpenParens + 1,
                    inlineParameterPolicy.Length - indexOfFirstOpenParens - 2);
            }
            else
            {
                parameterPolicyKey = inlineParameterPolicy;
                argumentString = null;
            }

            if (!inlineParameterPolicyMap.TryGetValue(parameterPolicyKey, out var parameterPolicyType))
            {
                return default;
            }

            if (!typeof(T).IsAssignableFrom(parameterPolicyType))
            {
                throw new RouteCreationException(
                            Resources.FormatDefaultInlineConstraintResolver_TypeNotConstraint(
                                                        parameterPolicyType, parameterPolicyKey, typeof(T).Name));
            }

            try
            {
                return (T)CreateParameterPolicy(serviceProvider, parameterPolicyType, argumentString);
            }
            catch (RouteCreationException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new RouteCreationException(
                    $"An error occurred while trying to create an instance of '{parameterPolicyType.FullName}'.",
                    exception);
            }
        }

        private static IParameterPolicy CreateParameterPolicy(IServiceProvider serviceProvider, Type constraintType, string argumentString)
        {
            // No arguments - call the default constructor
            if (argumentString == null)
            {
                return (IParameterPolicy)Activator.CreateInstance(constraintType);
            }

            var constraintTypeInfo = constraintType.GetTypeInfo();
            ConstructorInfo activationConstructor = null;
            object[] parameters = null;
            var constructors = constraintTypeInfo.DeclaredConstructors.ToArray();

            // If there is only one constructor and it has a single parameter, pass the argument string directly
            // This is necessary for the Regex RouteConstraint to ensure that patterns are not split on commas.
            if (constructors.Length == 1 && GetNonConvertableParameterTypeCount(serviceProvider, constructors[0].GetParameters()) == 1)
            {
                activationConstructor = constructors[0];
                parameters = ConvertArguments(serviceProvider, activationConstructor.GetParameters(), new string[] { argumentString });
            }
            else
            {
                var arguments = argumentString.Split(',').Select(argument => argument.Trim()).ToArray();

                var matchingConstructors = constructors.Where(ci => GetNonConvertableParameterTypeCount(serviceProvider, ci.GetParameters()) == arguments.Length)
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
                    parameters = ConvertArguments(serviceProvider, activationConstructor.GetParameters(), arguments);
                }
                else
                {
                    throw new RouteCreationException(
                                Resources.FormatDefaultInlineConstraintResolver_AmbiguousCtors(
                                                       constraintTypeInfo.Name, arguments.Length));
                }
            }

            return (IParameterPolicy)activationConstructor.Invoke(parameters);
        }

        private static int GetNonConvertableParameterTypeCount(IServiceProvider serviceProvider, ParameterInfo[] parameters)
        {
            if (serviceProvider == null)
            {
                return parameters.Length;
            }

            var count = 0;
            for (var i = 0; i < parameters.Length; i++)
            {
                if (typeof(IConvertible).IsAssignableFrom(parameters[i].ParameterType))
                {
                    count++;
                }
            }

            return count;
        }

        private static object[] ConvertArguments(IServiceProvider serviceProvider, ParameterInfo[] parameterInfos, string[] arguments)
        {
            var parameters = new object[parameterInfos.Length];
            var argumentPosition = 0;
            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var parameter = parameterInfos[i];
                var parameterType = parameter.ParameterType;

                if (serviceProvider != null && !typeof(IConvertible).IsAssignableFrom(parameterType))
                {
                    parameters[i] = serviceProvider.GetRequiredService(parameterType);
                }
                else
                {
                    parameters[i] = Convert.ChangeType(arguments[argumentPosition], parameterType, CultureInfo.InvariantCulture);
                    argumentPosition++;
                }
            }

            return parameters;
        }
    }
}
