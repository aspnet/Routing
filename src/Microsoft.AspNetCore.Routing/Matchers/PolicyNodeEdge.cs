// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    public readonly struct PolicyNodeEdge
    {
        public PolicyNodeEdge(object state, IReadOnlyList<Endpoint> endpoints)
        {
            State = state;
            Endpoints = endpoints;
        }

        public IReadOnlyList<Endpoint> Endpoints { get; }

        public object State { get; }
    }
}
