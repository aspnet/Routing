﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Routing
{
    public abstract class EndpointBuilder
    {
        public string DisplayName { get; set; }

        public IList<object> Metadata { get; } = new List<object>();

        public abstract Endpoint Build();
    }
}
