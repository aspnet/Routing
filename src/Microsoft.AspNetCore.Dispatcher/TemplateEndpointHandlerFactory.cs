﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Dispatcher
{
    public sealed class TemplateEndpointHandlerFactory : IHandlerFactory
    {
        public Func<RequestDelegate, RequestDelegate> CreateHandler(Endpoint endpoint)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            if (endpoint is TemplateEndpoint templateEndpoint)
            {
                return templateEndpoint.HandlerFactory;
            }

            return null;
        }
    }
}
