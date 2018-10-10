// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RoutingSample.Web.DomainPolicy;

namespace Microsoft.AspNetCore.Builder
{
    public static class DomainEndpointConventionBuilderExtensions
    {
        public static IEndpointConventionBuilder RequireDomain(this IEndpointConventionBuilder builder, params HostString[] domains)
        {
            builder.Apply(endpointBuilder => endpointBuilder.Metadata.Add(new DomainMetadata(domains)));
            return builder;
        }
    }
}