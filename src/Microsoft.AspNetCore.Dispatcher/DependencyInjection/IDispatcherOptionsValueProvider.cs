// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IDispatcherOptionsValueProvider
    {
        RequestDelegate Dispatcher { get; }
    }
}
