// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Routing
{
    internal class BuilderEndpointDataSource : EndpointDataSource, IApplyEndpointBuilder
    {
        internal IReadOnlyList<EndpointBuilder> EndpointBuilders { get; }
        internal List<Action<EndpointBuilder>> OnApply { get; }

        public BuilderEndpointDataSource(IReadOnlyList<EndpointBuilder> endpointBuilders)
        {
            if (endpointBuilders == null)
            {
                throw new ArgumentNullException(nameof(endpointBuilders));
            }

            EndpointBuilders = endpointBuilders;
            OnApply = new List<Action<EndpointBuilder>>();
        }

        public override IChangeToken GetChangeToken()
        {
            return NullChangeToken.Singleton;
        }

        public void Apply(Action<EndpointBuilder> apply)
        {
            if (apply == null)
            {
                throw new ArgumentNullException(nameof(apply));
            }

            OnApply.Add(apply);
        }

        private EndpointBuilder ApplyAll(EndpointBuilder endpointBuilder)
        {
            foreach (var apply in OnApply)
            {
                apply(endpointBuilder);
            }

            return endpointBuilder;
        }

        public override IReadOnlyList<Endpoint> Endpoints => EndpointBuilders.Select(e => ApplyAll(e).Build()).ToArray();
    }
}