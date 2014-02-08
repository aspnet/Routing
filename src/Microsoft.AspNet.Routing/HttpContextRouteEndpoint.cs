﻿﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Abstractions;

namespace Microsoft.AspNet.Routing
{
    public class HttpContextRouteEndpoint : IRouteEndpoint
    {
        private readonly RequestDelegate _appFunc;

        public HttpContextRouteEndpoint(RequestDelegate appFunc)
        {
            _appFunc = appFunc;
        }

        public async Task<bool> Send(HttpContext context)
        {
            await _appFunc(context);
            return true;
        }
    }
}
