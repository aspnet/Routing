// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal class ZeroEntryJumpTable : JumpTable
    {
        private readonly int _zeroExit;
        private readonly int _defaultExit;

        public ZeroEntryJumpTable(int zeroExit, int defaultExit)
        {
            _zeroExit = zeroExit;
            _defaultExit = defaultExit;
        }

        public override int GetDestination(ReadOnlySpan<char> segment)
        {
            return segment.Length == 0 ? _zeroExit : _defaultExit;
        }
    }
}
