// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Routing.Logging.Internal
{
    public static class LoggerExtensions
    {
        public static void WriteValues(this ILogger logger, object values)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            logger.Log(
                logLevel: LogLevel.Verbose,
                eventId: 0,
                state: values,
                exception: null,
                formatter: LogFormatter.Formatter);
        }
    }
}