﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Dispatcher
{
    public abstract class DispatcherBase : IDispatcherOptionsValueProvider
    {
        private readonly DispatcherBaseServices _services;

        private ILogger _logger;

        public DispatcherBase(DispatcherBaseServices services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            _services = services;
        }

        protected ILogger Logger
        {
            get
            {
                LazyInitializer.EnsureInitialized(ref _logger, () => _services.LoggerFactory.CreateLogger(GetType()));
                return _logger;
            }
        }

        public abstract Task InvokeAsync(HttpContext httpContext);

        RequestDelegate IDispatcherOptionsValueProvider.Dispatcher => InvokeAsync;
    }
}
