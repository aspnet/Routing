// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Routing.Matching
{
    internal class PolicyJumpTableBuilder
    {
        private readonly INodeBuilderPolicy _nodeBuilder;
        private PolicyJumpTableEdge[] _entries;

        public PolicyJumpTableBuilder(INodeBuilderPolicy nodeBuilder)
        {
            _nodeBuilder = nodeBuilder;
        }

        // The destination state for a non-match.
        public int ExitDestination { get; set; } = JumpTableBuilder.InvalidDestination;

        public void AddEntries(PolicyJumpTableEdge[] entries)
        {
            _entries = entries;
        }

        public PolicyJumpTable Build()
        {
            return _nodeBuilder.BuildJumpTable(ExitDestination, _entries);
        }
    }
}
