﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Dispatcher;

namespace Microsoft.AspNetCore.Routing.Dispatcher
{
    public abstract class RouterDispatcherFactory
    {
        public abstract DispatcherBase CreateDispatcher(string routeTemplate, RouteValuesEndpoint endpoint, params RouteValuesEndpoint[] additionalEndpoints);
    }
}
