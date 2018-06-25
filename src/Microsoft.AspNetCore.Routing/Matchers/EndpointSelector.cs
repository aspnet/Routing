// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    public abstract class EndpointSelector2
    {
        public abstract Task Select(EndpointSelectorContext context);
    }
}
