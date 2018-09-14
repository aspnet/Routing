// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNetCore.Routing
{
    public abstract class EndpointDataSourcesBuilder
    {
        public abstract IApplicationBuilder CreateApplicationBuilder();

        public abstract IServiceProvider ServiceProvider { get; }

        public abstract IList<EndpointDataSource> EndpointDataSources { get; }
    }
}