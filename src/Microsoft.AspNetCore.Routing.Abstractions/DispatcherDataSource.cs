// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Routing
{
    public abstract class DispatcherDataSource : IEndpointCollectionProvider
    {
        public abstract IChangeToken ChangeToken { get; }

        protected abstract IReadOnlyList<Endpoint> GetEndpoints();

        IReadOnlyList<Endpoint> IEndpointCollectionProvider.Endpoints => GetEndpoints();
    }
}