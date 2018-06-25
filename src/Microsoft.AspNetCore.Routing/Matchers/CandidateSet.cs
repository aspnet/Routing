﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    public sealed class CandidateSet
    {
        private CandidateState _state0;
        private CandidateState _state1;
        private CandidateState _state2;
        private CandidateState _state3;

        private CandidateState[] _additionalCandidates;

        // Provided to make testing possible/easy.
        public CandidateSet(MatcherEndpoint[] endpoints)
        {
            Count = endpoints.Length;

            switch (endpoints.Length)
            {
                case 0:
                    return;

                case 1:
                    _state0 = new CandidateState(endpoints[0], score: 0);
                    break;

                case 2:
                    _state0 = new CandidateState(endpoints[0], score: 0);
                    _state1 = new CandidateState(endpoints[1], score: 0);
                    break;

                case 3:
                    _state0 = new CandidateState(endpoints[0], score: 0);
                    _state1 = new CandidateState(endpoints[1], score: 0);
                    _state2 = new CandidateState(endpoints[2], score: 0);
                    break;

                case 4:
                    _state0 = new CandidateState(endpoints[0], score: 0);
                    _state1 = new CandidateState(endpoints[1], score: 0);
                    _state2 = new CandidateState(endpoints[2], score: 0);
                    _state3 = new CandidateState(endpoints[3], score: 0);
                    break;

                default:
                    _state0 = new CandidateState(endpoints[0], score: 0);
                    _state1 = new CandidateState(endpoints[1], score: 0);
                    _state2 = new CandidateState(endpoints[2], score: 0);
                    _state3 = new CandidateState(endpoints[3], score: 0);

                    _additionalCandidates = new CandidateState[endpoints.Length - 4];
                    for (var i = 4; i < endpoints.Length; i++)
                    {
                        _additionalCandidates[i - 4] = new CandidateState(endpoints[i], score: 0);
                    }
                    break;
            }
        }

        internal CandidateSet(Candidate[] candidates)
        {
            Count = candidates.Length;

            switch (candidates.Length)
            {
                case 0:
                    return;

                case 1:
                    _state0 = new CandidateState(candidates[0].Endpoint, candidates[0].Score);
                    break;

                case 2:
                    _state0 = new CandidateState(candidates[0].Endpoint, candidates[0].Score);
                    _state1 = new CandidateState(candidates[1].Endpoint, candidates[1].Score);
                    break;

                case 3:
                    _state0 = new CandidateState(candidates[0].Endpoint, candidates[0].Score);
                    _state1 = new CandidateState(candidates[1].Endpoint, candidates[1].Score);
                    _state2 = new CandidateState(candidates[2].Endpoint, candidates[2].Score);
                    break;

                case 4:
                    _state0 = new CandidateState(candidates[0].Endpoint, candidates[0].Score);
                    _state1 = new CandidateState(candidates[1].Endpoint, candidates[1].Score);
                    _state2 = new CandidateState(candidates[2].Endpoint, candidates[2].Score);
                    _state3 = new CandidateState(candidates[3].Endpoint, candidates[3].Score);
                    break;

                default:
                    _state0 = new CandidateState(candidates[0].Endpoint, candidates[0].Score);
                    _state1 = new CandidateState(candidates[1].Endpoint, candidates[1].Score);
                    _state2 = new CandidateState(candidates[2].Endpoint, candidates[2].Score);
                    _state3 = new CandidateState(candidates[3].Endpoint, candidates[3].Score);

                    _additionalCandidates = new CandidateState[candidates.Length - 4];
                    for (var i = 4; i < candidates.Length; i++)
                    {
                        _additionalCandidates[i - 4] = new CandidateState(candidates[i].Endpoint, candidates[i].Score);
                    }
                    break;
            }
        }

        public int Count { get; }

        public ref CandidateState this[int index]
        {
            // PERF: Force inlining
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                // Friendliness for inlining
                if ((uint)index >= Count)
                {
                    ThrowIndexArgumentOutOfRangeException();
                }

                switch (index)
                {
                    case 0:
                        return ref _state0;

                    case 1:
                        return ref _state1;

                    case 2:
                        return ref _state2;

                    case 3:
                        return ref _state3;

                    default:
                        return ref _additionalCandidates[index - 4];
                }
            }
        }

        private static void ThrowIndexArgumentOutOfRangeException()
        {
            throw new ArgumentOutOfRangeException("index");
        }
    }
}
