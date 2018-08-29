// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using System;

namespace Microsoft.AspNetCore.Routing
{
    public static class LinkGeneratorEndpointNameAddressExtensions
    {
        public static string GetPathByName(
            this LinkGenerator generator,
            HttpContext httpContext,
            string endpointName,
            object values,
            FragmentString fragment = default,
            LinkOptions options = default)
        {
            if (generator == null)
            {
                throw new ArgumentNullException(nameof(generator));
            }

            if (endpointName == null)
            {
                throw new ArgumentNullException(nameof(endpointName));
            }

            return generator.GetPathByAddress<string>(httpContext, endpointName, new RouteValueDictionary(values), fragment, options);
        }

        public static string GetPathByName(
            this LinkGenerator generator,
            string endpointName,
            object values,
            PathString pathBase = default,
            FragmentString fragment = default,
            LinkOptions options = default)
        {
            if (generator == null)
            {
                throw new ArgumentNullException(nameof(generator));
            }

            if (endpointName == null)
            {
                throw new ArgumentNullException(nameof(endpointName));
            }

            return generator.GetPathByAddress<string>(endpointName, new RouteValueDictionary(values), pathBase, fragment, options);
        }

        public static string GetUriByName(
            this LinkGenerator generator,
            HttpContext httpContext,
            string endpointName,
            object values,
            FragmentString fragment = default,
            LinkOptions options = default)
        {
            if (endpointName == null)
            {
                throw new ArgumentNullException(nameof(endpointName));
            }

            return generator.GetUriByAddress<string>(httpContext, endpointName, new RouteValueDictionary(values), fragment, options);
        }

        public static string GetUriByName(
            this LinkGenerator generator,
            string endpointName,
            object values,
            string scheme,
            HostString host,
            PathString pathBase = default,
            FragmentString fragment = default,
            LinkOptions options = default)
        {
            if (endpointName == null)
            {
                throw new ArgumentNullException(nameof(endpointName));
            }

            return generator.GetUriByAddress<string>(endpointName, new RouteValueDictionary(values), scheme, host, pathBase, fragment, options);
        }
    }
}
