// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    public class EndpointSelectorContext
    {
        private readonly BitArray _members;
        private readonly Memory<Candidate> _candidates;
        private readonly RouteValueDictionary[] _routeValues;

        internal EndpointSelectorContext(
            IEndpointFeature endpointFeature,
            Memory<Candidate> candidates,
            RouteValueDictionary[] routeValues,
            BitArray members)
        {
            EndpointFeature = endpointFeature;
            _candidates = candidates;
            _routeValues = routeValues;
            _members = members;
        }

        public int Count => _candidates.Length;

        public IEndpointFeature EndpointFeature { get; }

        public bool IsCandidate(int index)
        {
            if ((uint)index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _members.Get(index);
        }

        public MatcherEndpoint GetEndpoint(int index)
        {
            if ((uint)index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            
            return _candidates.Span[index].Endpoint;
        }

        public RouteValueDictionary GetValues(int index)
        {
            if ((uint)index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _routeValues[index];
        }
    }
}
