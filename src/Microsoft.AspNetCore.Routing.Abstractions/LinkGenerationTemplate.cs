// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing
{
    /// <summary>
    /// Defines a contract to generate a URL from a template.
    /// </summary>
    public abstract class LinkGenerationTemplate
    {
        public abstract string GetPath(
            HttpContext httpContext,
            object values,
            FragmentString fragment = default,
            LinkOptions options = default);

        public abstract string GetPath(
            object values,
            PathString pathBase = default,
            FragmentString fragment = default,
            LinkOptions options = default);

        public abstract string GetUri(
            HttpContext httpContext,
            object values,
            FragmentString fragment = default,
            LinkOptions options = default);

        public abstract string GetUri(
            object values,
            string scheme,
            HostString host,
            PathString pathBase = default,
            FragmentString fragment = default,
            LinkOptions options = default);
    }
}