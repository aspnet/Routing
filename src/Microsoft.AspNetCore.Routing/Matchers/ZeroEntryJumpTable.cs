// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal class ZeroEntryJumpTable : JumpTable
    {
        private readonly int _default;
        private readonly int _exit;

        public ZeroEntryJumpTable(int @default, int exit)
        {
            _default = @default;
            _exit = exit;
        }

        public unsafe override int GetDestination(string path, PathSegment segment)
        {
            return segment.Length == 0 ? _exit : _default;
        }

        public override string DebuggerToString()
        {
            return $"{{ $default: {_default}, $0: {_exit} }}";
        }
    }
}
