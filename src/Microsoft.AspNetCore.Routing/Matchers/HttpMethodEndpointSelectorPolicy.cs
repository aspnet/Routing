// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    public sealed class HttpMethodEndpointSelectorPolicy : EndpointSelectorPolicy
    {
        public override object GetMetadata(Endpoint endpoint)
        {
            return endpoint.Metadata.GetMetadata<IHttpMethodMetadata>();
        }

        public override bool Match(
            HttpContext httpContext,
            Endpoint endpoint,
            object metadata,
            RouteValueDictionary values)
        {
            if (metadata == null)
            {
                return true;
            }

            var httpMethod = httpContext.Request.Method;
            var httpMethods = ((IHttpMethodMetadata)metadata).HttpMethods;
            for (var i = 0; i < httpMethods.Count; i++)
            {
                if (string.Equals(httpMethods[i], httpMethod, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public override void Reject(
            HttpContext httpContext,
            IEndpointFeature feature,
            IReadOnlyList<Endpoint> endpoints)
        {
            var allow = string.Join(
                ", ",
                endpoints
                .Select(e => e.Metadata.GetMetadata<IHttpMethodMetadata>())
                .Where(m => m != null)
                .SelectMany(m => m.HttpMethods)
                .Distinct());

            feature.Invoker = (next) => (context) =>
            {
                context.Response.StatusCode = 405;
                context.Response.Headers.Add("Allow", allow);
                return Task.CompletedTask;
            };
        }
    }
}
