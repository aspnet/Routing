// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class ConfigureEndpointOptions : IConfigureOptions<EndpointOptions>
    {
        private readonly IEnumerable<EndpointDataSource> _dataSources;
        private readonly IEnumerable<EndpointDataSourcesBuilder> _dataSourcesBuilders;

        public ConfigureEndpointOptions(IEnumerable<EndpointDataSource> dataSources, IEnumerable<EndpointDataSourcesBuilder> dataSourcesBuilders)
        {
            if (dataSources == null)
            {
                throw new ArgumentNullException(nameof(dataSources));
            }

            if (dataSourcesBuilders == null)
            {
                throw new ArgumentNullException(nameof(dataSourcesBuilders));
            }

            _dataSources = dataSources;
            _dataSourcesBuilders = dataSourcesBuilders;
        }

        public void Configure(EndpointOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            foreach (var dataSource in _dataSources)
            {
                options.DataSources.Add(dataSource);
            }

            foreach (var dataSourceBuilder in _dataSourcesBuilders)
            {
                foreach (var dataSource in dataSourceBuilder.EndpointDataSources)
                {
                    options.DataSources.Add(dataSource);
                }
            }
        }
    }
}