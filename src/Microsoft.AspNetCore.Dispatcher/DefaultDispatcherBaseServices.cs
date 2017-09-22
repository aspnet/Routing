// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Dispatcher
{
    internal class DefaultDispatcherBaseServices : DispatcherBaseServices
    {
        public DefaultDispatcherBaseServices(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
        }
    }
}
