// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Routing
{
    public class LinkGeneratorContext
    {
        public Address Address { get; set; }

        public IDictionary<string, object> AmbientValues { get; set; }

        public IDictionary<string, object> SuppliedValues { get; set; }
    }
}
