// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace RoutingSample.Web.DomainPolicy
{
    internal class DomainMetadata
    {
        public DomainMetadata(IEnumerable<HostString> domains)
        {
            Domains = domains.ToList();
        }

        public IReadOnlyList<HostString> Domains { get; }
    }
}
