﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal readonly struct Candidate
    {
        public readonly MatcherEndpoint Endpoint;

        // Used to optimize out operations that modify route values.
        public readonly CandidateFlags Flags;

        // Data for creating the RouteValueDictionary. We assign each key its own slot
        // and we fill the values array with all of the default values.
        //
        // Then when we process parameters, we don't need to operate on the RouteValueDictionary
        // we can just operate on an array, which is much much faster.
        public readonly KeyValuePair<string, object>[] Slots;

        // List of parameters to capture. Segment is the segment index, index is the 
        // index into the values array.
        public readonly (string parameterName, int segmentIndex, int slotIndex)[] Captures;

        // Catchall parameter to capture (limit one per template).
        public readonly (string parameterName, int segmentIndex, int slotIndex) CatchAll;

        // Complex segments are processed in a separate pass because they require a
        // RouteValueDictionary.
        public readonly (RoutePatternPathSegment pathSegment, int segmentIndex)[] ComplexSegments;

        public readonly MatchProcessor[] MatchProcessors;

        // Data for EndpointSelectorPolicy - indexed by policy index in the DFA matcher
        public readonly object[] PolicyData;

        // Used in tests.
        public Candidate(MatcherEndpoint endpoint)
        {
            Endpoint = endpoint;

            Slots = Array.Empty<KeyValuePair<string, object>>();
            Captures = Array.Empty<(string parameterName, int segmentIndex, int slotIndex)>();
            CatchAll = default;
            ComplexSegments = Array.Empty<(RoutePatternPathSegment pathSegment, int segmentIndex)>();
            MatchProcessors = Array.Empty<MatchProcessor>();
            PolicyData = Array.Empty<object>();

            Flags = CandidateFlags.None;
        }

        public Candidate(
            MatcherEndpoint endpoint,
            KeyValuePair<string, object>[] slots,
            (string parameterName, int segmentIndex, int slotIndex)[] captures,
            (string parameterName, int segmentIndex, int slotIndex) catchAll,
            (RoutePatternPathSegment pathSegment, int segmentIndex)[] complexSegments,
            MatchProcessor[] matchProcessors,
            object[] policyData)
        {
            Endpoint = endpoint;
            Slots = slots;
            Captures = captures;
            CatchAll = catchAll;
            ComplexSegments = complexSegments;
            MatchProcessors = matchProcessors;
            PolicyData = policyData;

            Flags = CandidateFlags.None;
            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i].Key != null)
                {
                    Flags |= CandidateFlags.HasDefaults;
                }
            }

            if (captures.Length > 0)
            {
                Flags |= CandidateFlags.HasCaptures;
            }

            if (catchAll.parameterName != null)
            {
                Flags |= CandidateFlags.HasCatchAll;
            }

            if (complexSegments.Length > 0)
            {
                Flags |= CandidateFlags.HasComplexSegments;
            }

            if (matchProcessors.Length > 0)
            {
                Flags |= CandidateFlags.HasMatchProcessors;
            }
        }

        [Flags]
        public enum CandidateFlags
        {
            None = 0,
            HasDefaults = 1,
            HasCaptures = 2,
            HasCatchAll = 4,
            HasSlots = HasDefaults | HasCaptures | HasCatchAll,
            HasComplexSegments = 8,
            HasMatchProcessors = 16,
        }
    }
}
