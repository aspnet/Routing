// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal class InstructionMatcher : MatcherBase
    {
        private State _state;

        public InstructionMatcher(Instruction[] instructions, Candidate[] candidates, JumpTable[] tables)
        {
            _state = new State()
            {
                Instructions = instructions,
                Candidates = candidates,
                Tables = tables,
            };
        }

        protected override void SelectCandidates(HttpContext httpContext, ref CandidateSet candidates)
        {
            var state = _state;

            var i = 0;
            var matches = new List<int>();
            while (i < state.Instructions.Length)
            {
                var instruction = state.Instructions[i];
                switch (instruction.Code)
                {
                    case InstructionCode.Accept:
                        {
                            if (candidates.Segments.Length == instruction.Depth)
                            {
                                matches.Add(instruction.Payload);
                            }
                            i++;
                            break;
                        }
                    case InstructionCode.Branch:
                        {
                            var table = state.Tables[instruction.Payload];
                            i = table.GetDestination(candidates.Path, candidates.Segments[instruction.Depth]);
                            break;
                        }
                    case InstructionCode.Jump:
                        {
                            i = instruction.Payload;
                            break;
                        }
                }
            }

            candidates.Candidates = state.Candidates;
            candidates.CandidateIndices = matches.ToArray();
            candidates.CandidateGroups = new int[] { matches.Count, };
        }

        public class State
        {
            public Candidate[] Candidates;
            public Instruction[] Instructions;
            public JumpTable[] Tables;
        }

        [DebuggerDisplay("{ToDebugString(),nq}")]
        [StructLayout(LayoutKind.Explicit)]
        public struct Instruction
        {
            [FieldOffset(0)]
            public byte Depth;

            [FieldOffset(3)]
            public InstructionCode Code;

            [FieldOffset(4)]
            public int Payload;

            private string ToDebugString()
            {
                return $"{Code}: {Payload}";
            }
        }

        public enum InstructionCode : byte
        {
            Accept,
            Branch,
            Jump,
            Pop, // Only used during the instruction builder phase
        }
    }
}
