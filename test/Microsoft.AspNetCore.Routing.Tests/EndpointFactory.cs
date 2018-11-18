// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.TestObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Routing
{
    internal static class EndpointFactory
    {
        public static RouteEndpoint CreateRouteEndpoint(
            string template,
            object defaults = null,
            object policies = null,
            object requiredValues = null,
            int order = 0,
            string displayName = null,
            params object[] metadata)
        {
            var d = new List<object>(metadata ?? Array.Empty<object>());

            var routePattern = RoutePatternFactory.Parse(template, defaults, policies);

            if (requiredValues != null)
            {
                var policyFactory = CreateParameterPolicyFactory();
                var defaultRoutePatternTransformer = new DefaultRoutePatternTransformer(policyFactory);

                routePattern = defaultRoutePatternTransformer.SubstituteRequiredValues(routePattern, requiredValues);
            }

            return new RouteEndpoint(
                TestConstants.EmptyRequestDelegate,
                routePattern,
                order,
                new EndpointMetadataCollection(d),
                displayName);
        }

        private static DefaultParameterPolicyFactory CreateParameterPolicyFactory()
        {
            var serviceCollection = new ServiceCollection();
            var policyFactory = new DefaultParameterPolicyFactory(
                Options.Create(new RouteOptions
                {
                    ConstraintMap =
                    {
                        ["slugify"] = typeof(SlugifyParameterTransformer),
                        ["upper-case"] = typeof(UpperCaseParameterTransform)
                    }
                }),
                serviceCollection.BuildServiceProvider());

            return policyFactory;
        }
    }
}
