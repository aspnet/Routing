﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Routing.Template;

namespace Microsoft.AspNetCore.Dispatcher
{
    public class DispatcherEntry
    {
        public IList<RouteValuesEndpoint> Endpoints { get; set; }

        public RouteTemplate RouteTemplate { get; set; }
    }
}
