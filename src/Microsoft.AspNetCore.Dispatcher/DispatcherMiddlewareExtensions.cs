// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNetCore.Dispatcher
{
    public static class DispatcherMiddlewareExtensions
    {
        public static IApplicationBuilder UseDispatcher(this IApplicationBuilder builder)
        {
            builder.Properties.Add("Dispatcher", true);
            return builder.UseMiddleware<DispatcherMiddleware>();
        }
    }
}
