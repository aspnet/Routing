// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing
{
    public abstract class EndpointSelectorPolicy
    {
        public abstract bool Match(
            HttpContext httpContext,
            Endpoint endpoint,
            object metadata,
            RouteValueDictionary values);

        public virtual void Reject(
            HttpContext httpContext,
            IEndpointFeature feature,
            IReadOnlyList<Endpoint> endpoints)
        {
        }

        public virtual object GetMetadata(Endpoint endpoint)
        {
            return null;
        }

        public virtual int CompareMetadata(object x, object y)
        {
            return DefaultCompare(x, y);
        }

        private protected static int DefaultCompare(object x, object y)
        {
            // The default policy is that if x endpoint defines TMetadata, and
            // y endpoint does not, then x is *more specific* than y. We return
            // -1 for this case so that x will come first in the sort order.

            if (x == null && y != null)
            {
                // y is more specific
                return 1;
            }
            else if (x != null && y == null)
            {
                // x is more specific
                return -1;
            }

            // both endpoints have this metadata, or both do not have it, they have
            // the same specificity.
            return 0;
        }
    }

    internal class EndpointMetadataComparer : IComparer<Endpoint>
    {
        private readonly EndpointSelectorPolicy _selector;

        public EndpointMetadataComparer(EndpointSelectorPolicy selector)
        {
            _selector = selector;
        }

        public int Compare(Endpoint x, Endpoint y)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            return _selector.CompareMetadata(_selector.GetMetadata(x), _selector.GetMetadata(y));
        }
    }

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

    public sealed class HttpMethodMetadata : IHttpMethodMetadata
    {
        public HttpMethodMetadata(IEnumerable<string> httpMethods)
        {
            HttpMethods = httpMethods.ToArray();
        }

        public IReadOnlyList<string> HttpMethods { get; }
    }

    public interface IHttpMethodMetadata
    {
        IReadOnlyList<string> HttpMethods { get; }
    }
}
