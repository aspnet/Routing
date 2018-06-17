// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal class LinearSearchJumpTable : JumpTable
    {
        private readonly (string text, int destination)[] _entries;
        private readonly int _zeroExit;
        private readonly int _defaultExit;

        public LinearSearchJumpTable((string text, int destination)[] entries, int zeroExit, int defaultExit)
        {
            _entries = entries;
            _zeroExit = zeroExit;
            _defaultExit = defaultExit;
        }

        public override int GetDestination(ReadOnlySpan<char> segment)
        {
            if (segment.Length == 0)
            {
                return _zeroExit;
            }

            for (var i = 0; i < _entries.Length;i++)
            {
                ref var entry = ref _entries[i];
                if (segment.Length == entry.text.Length && // yes, checking the length up front is beneficial.
                    segment.Equals(entry.text.AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    return entry.destination;
                }
            }

            return _defaultExit;
        }
    }
}
