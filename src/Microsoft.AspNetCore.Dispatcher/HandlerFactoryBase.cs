// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Dispatcher
{
    public abstract class HandlerFactoryBase : IHandlerFactoryOptionsValueProvider
    {
        private readonly HandlerFactoryBaseServices _services;

        private ILogger _logger;

        public HandlerFactoryBase(HandlerFactoryBaseServices services)
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
        
        public abstract Func<RequestDelegate, RequestDelegate> CreateHandler(Endpoint endpoint);

        EndpointHandlerFactory IHandlerFactoryOptionsValueProvider.HandlerFactory => CreateHandler;
    }
}
