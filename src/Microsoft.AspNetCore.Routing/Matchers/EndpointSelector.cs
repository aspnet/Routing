﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    public abstract class EndpointSelector
    {
        public abstract Task SelectAsync(
            HttpContext httpContext,
            IEndpointFeature feature,
            CandidateSet candidates);
    }
}
