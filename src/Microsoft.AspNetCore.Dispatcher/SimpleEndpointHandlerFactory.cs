// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Dispatcher
{
    public class SimpleEndpointHandlerFactory : HandlerFactoryBase
    {
        public SimpleEndpointHandlerFactory(HandlerFactoryBaseServices services) 
            : base(services)
        {
        }

        public override Func<RequestDelegate, RequestDelegate> CreateHandler(Endpoint endpoint)
        {
            return (endpoint as SimpleEndpoint).DelegateFactory;
        }
    }
}
