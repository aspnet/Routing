// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Routing
{
    public interface IRouteCollectionBuilder
    {
        IRouter DefaultHandler { get; }

        IServiceProvider ServiceProvider { get; }

        IRouteCollection Routes { get; }
    }
}