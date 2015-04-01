// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNet.Routing
{
    public interface IRouter
    {
        Task RouteAsync(RouteContext context);

// tell pranav to ignore this again
        VirtualPathData GetVirtualPath(VirtualPathContext context);
    }
}
