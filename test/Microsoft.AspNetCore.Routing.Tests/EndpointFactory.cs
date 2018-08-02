// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.Routing.Patterns;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Routing
{
    internal static class EndpointFactory
    {
        public static MatcherEndpoint CreateMatcherEndpoint(
            string template,
            object defaults = null,
            object constraints = null,
            object requiredValues = null,
            int order = 0,
            string displayName = null,
            params object[] metadata)
        {
            var d = new List<object>(metadata ?? Array.Empty<object>());
            if (requiredValues != null)
            {
                d.Add(new RequiredValuesMetadata(new RouteValueDictionary(requiredValues)));
            }

            return new MatcherEndpoint(
                MatcherEndpoint.EmptyInvoker,
                RoutePatternFactory.Parse(template, defaults, constraints),
                order,
                new EndpointMetadataCollection(d),
                displayName);
        }

        private class RequiredValuesMetadata : IRequiredValuesMetadata
        {
            public RequiredValuesMetadata(IReadOnlyDictionary<string, object> requiredValues)
            {
                RequiredValues = requiredValues;
            }
            public IReadOnlyDictionary<string, object> RequiredValues { get; }
        }
    }
}
