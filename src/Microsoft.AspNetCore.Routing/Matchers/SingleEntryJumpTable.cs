// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal class SingleEntryJumpTable : JumpTable
    {
        private readonly int _default;
        private readonly int _exit;
        private readonly string _text;
        private readonly int _destination;

        public SingleEntryJumpTable(int @default, int exit, string text, int destination)
        {
            _default = @default;
            _exit = exit;
            _text = text;
            _destination = destination;
        }

        public override int GetDestination(string path, PathSegment segment)
        {
            if (segment.Length == 0)
            {
                return _exit;
            }

            if (segment.Length == _text.Length &&
                string.Compare(
                    path,
                    segment.Start,
                    _text,
                    0,
                    segment.Length,
                    StringComparison.OrdinalIgnoreCase) == 0)
            {
                return _destination;
            }

            return _default;
        }

        public override string DebuggerToString()
        {
            return $"{{ {_text}: {_destination}, $default: {_default}, $0: {_exit} }}";
        }
    }
}
