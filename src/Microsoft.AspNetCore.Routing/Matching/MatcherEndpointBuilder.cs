// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Microsoft.AspNetCore.Routing.Matching
{
    public class MatcherEndpointBuilder : EndpointBuilder
    {
        private readonly Func<RequestDelegate, RequestDelegate> _invoker;
        private readonly RoutePattern _routePattern;
        private readonly RouteValueDictionary _requiredValues;
        private readonly int _order;

        public MatcherEndpointBuilder(
           Func<RequestDelegate, RequestDelegate> invoker,
           RoutePattern routePattern,
           RouteValueDictionary requiredValues,
           int order)
        {
            _invoker = invoker;
            _routePattern = routePattern;
            _requiredValues = requiredValues;
            _order = order;
        }

        public override Endpoint Build()
        {
            var resolvedMetadata = new List<object>(Metadata);
            resolvedMetadata.Add(new RouteValuesAddressMetadata(null, _requiredValues));

            var matcherEndpoint = new MatcherEndpoint(
                _invoker,
                _routePattern,
                _order,
                new EndpointMetadataCollection(resolvedMetadata),
                DisplayName);

            return matcherEndpoint;
        }
    }
}
