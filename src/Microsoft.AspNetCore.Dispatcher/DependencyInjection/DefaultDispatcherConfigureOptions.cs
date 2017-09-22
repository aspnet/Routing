// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Dispatcher;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class DefaultDispatcherConfigureOptions : IConfigureOptions<DispatcherOptions>
    {
        private readonly IEnumerable<IDispatcherOptionsValueProvider> _dispatcherProviders;
        private readonly IEnumerable<IHandlerFactoryOptionsValueProvider> _handlerFactoryProviders;

        public DefaultDispatcherConfigureOptions(
            IEnumerable<IDispatcherOptionsValueProvider> dispatcherProviders,
            IEnumerable<IHandlerFactoryOptionsValueProvider> handlerFactoryProviders)
        {
            _dispatcherProviders = dispatcherProviders;
            _handlerFactoryProviders = handlerFactoryProviders;
        }

        public void Configure(DispatcherOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            foreach (var dispatcherProvider in _dispatcherProviders)
            {
                options.Dispatchers.Add(dispatcherProvider.Dispatcher);
            }

            foreach (var handlerFactoryProvider in _handlerFactoryProviders)
            {
                options.HandlerFactories.Add(handlerFactoryProvider.HandlerFactory);
            }
        }
    }
}
