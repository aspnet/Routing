// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.Encodings.Web;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Routing.Internal
{
    public class UriBuilderContextPooledObjectPolicy : IPooledObjectPolicy<UriBuildingContext>
    {
        private readonly RouteOptions _options;

        public UriBuilderContextPooledObjectPolicy(IOptions<RouteOptions> routeOptions)
        {
            _options = routeOptions.Value;
        }

        public UriBuildingContext Create()
        {
            return new UriBuildingContext(UrlEncoder.Default, _options);
        }

        public bool Return(UriBuildingContext obj)
        {
            obj.Clear();
            return true;
        }
    }
}
