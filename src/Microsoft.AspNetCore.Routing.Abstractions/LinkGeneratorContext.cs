// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing
{
    public class LinkGeneratorContext
    {
        public HttpContext HttpContext { get; set; }

        public IEnumerable<Endpoint> Endpoints { get; set; }

        public RouteValueDictionary ExplicitValues { get; set; }

        public RouteValueDictionary AmbientValues { get; set; }

        public bool? LowercaseUrls { get; set; }

        public bool? LowercaseQueryStrings { get; set; }

        public bool? AppendTrailingSlash { get; set; }
    }
}
