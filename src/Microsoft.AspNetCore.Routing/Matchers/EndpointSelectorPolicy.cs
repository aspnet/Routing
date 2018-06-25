// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
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

        private static int DefaultCompare(object x, object y)
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
}
