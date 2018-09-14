// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNetCore.Routing
{
    internal class DefaultEndpointDataSourcesBuilder : EndpointDataSourcesBuilder
    {
        public DefaultEndpointDataSourcesBuilder()
        {
            EndpointDataSources = new List<EndpointDataSource>();
        }

        public IApplicationBuilder ApplicationBuilder { get; set; }

        public override IApplicationBuilder CreateApplicationBuilder() => ApplicationBuilder.New();

        public override IList<EndpointDataSource> EndpointDataSources { get; }

        public override IServiceProvider ServiceProvider => ApplicationBuilder.ApplicationServices;
    }
}