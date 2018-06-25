// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal class EndpointMetadataComparer : IComparer<Endpoint>
    {
        private readonly EndpointSelectorPolicy _selector;

        public EndpointMetadataComparer(EndpointSelectorPolicy selector)
        {
            _selector = selector ?? throw new ArgumentNullException(nameof(selector));
        }

        public int Compare(Endpoint x, Endpoint y)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            return _selector.CompareMetadata(_selector.GetMetadata(x), _selector.GetMetadata(y));
        }
    }
}
