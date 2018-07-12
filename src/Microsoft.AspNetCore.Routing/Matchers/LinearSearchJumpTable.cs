// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal class LinearSearchJumpTable : JumpTable
    {
        private readonly int _default;
        private readonly int _exit;
        private readonly (string text, int destination)[] _entries;

        public LinearSearchJumpTable(int @default, int exit, (string text, int destination)[] entries)
        {
            _default = @default;
            _exit = exit;
            _entries = entries;
        }

        public override int GetDestination(string path, PathSegment segment)
        {
            if (segment.Length == 0)
            {
                return _exit;
            }

            var entries = _entries;
            for (var i = 0; i < entries.Length; i++)
            {
                var text = entries[i].text;
                if (segment.Length == text.Length &&
                    string.Compare(
                        path,
                        segment.Start,
                        text,
                        0,
                        segment.Length,
                        StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return entries[i].destination;
                }
            }

            return _default;
        }
    }
}
