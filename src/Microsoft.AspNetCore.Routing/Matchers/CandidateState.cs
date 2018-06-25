// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Routing.Matchers
{
    public struct CandidateState
    {
        // Provided for testability
        public CandidateState(MatcherEndpoint endpoint)
        {
            Endpoint = endpoint;

            Score = 0;
            IsValidCandiate = true;
            Values = null;
        }

        public CandidateState(MatcherEndpoint endpoint, int score)
        {
            Endpoint = endpoint;
            Score = score;

            IsValidCandiate = true;
            Values = null;
        }

        public MatcherEndpoint Endpoint { get; }

        public int Score { get; }

        public bool IsValidCandiate { get; set; }

        public RouteValueDictionary Values { get; set; }
    }
}
