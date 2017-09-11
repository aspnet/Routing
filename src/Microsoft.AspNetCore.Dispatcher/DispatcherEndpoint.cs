﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Dispatcher
{
    public class DispatcherEndpoint : Endpoint
    {
        public DispatcherEndpoint(string displayName)
        {
            DisplayName = displayName;
        }

        public override string DisplayName { get; }
    }
}
