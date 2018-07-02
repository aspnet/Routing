// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal readonly struct Candidate
    {
        public Candidate(Endpoint endpoint, MatchProcessor[] segments)
        {
            Endpoint = endpoint;
            Processors = segments;
        }

        public readonly Endpoint Endpoint;
        public readonly MatchProcessor[] Processors;
    }
}
