// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Matching;

namespace Microsoft.AspNetCore.Routing
{
    internal class DefaultLinkGenerationTemplate : LinkGenerationTemplate
    {
        private readonly HttpContext _httpContext;
        private readonly RouteValueDictionary _earlierExplicitValues;
        private readonly RouteValueDictionary _ambientValues;
        private readonly DefaultLinkGenerator _linkGenerator;
        private readonly IEnumerable<MatcherEndpoint> _endpoints;

        public DefaultLinkGenerationTemplate(
            DefaultLinkGenerator linkGenerator,
            IEnumerable<MatcherEndpoint> endpoints,
            HttpContext httpContext,
            RouteValueDictionary explicitValues,
            RouteValueDictionary ambientValues)
        {
            _linkGenerator = linkGenerator;
            _endpoints = endpoints;
            _httpContext = httpContext;
            _earlierExplicitValues = explicitValues;
            _ambientValues = ambientValues;
        }

        public override string MakeUrl(object values, LinkOptions options)
        {
            var currentValues = new RouteValueDictionary(values);
            var mergedValuesDictionary = new RouteValueDictionary(_earlierExplicitValues);

            foreach (var kvp in currentValues)
            {
                mergedValuesDictionary[kvp.Key] = kvp.Value;
            }

            foreach (var endpoint in _endpoints)
            {
                var link = _linkGenerator.MakeLink(
                    _httpContext,
                    endpoint,
                    _ambientValues,
                    mergedValuesDictionary,
                    options);
                if (link != null)
                {
                    return link;
                }
            }
            return null;
        }
    }
}
