// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal class SingleEntryJumpTable : JumpTable
    {
        private readonly string _text;
        private readonly int _destination;
        private readonly int _zeroExit;
        private readonly int _defaultExit;

        public SingleEntryJumpTable(string text, int destination, int zeroExit, int defaultExit)
        {
            _text = text;
            _destination = destination;
            _zeroExit = zeroExit;
            _defaultExit = defaultExit;
        }

        public override int GetDestination(ReadOnlySpan<char> segment)
        {
            if (segment.Length == 0)
            {
                return _zeroExit;
            }
            else if (
                segment.Length == _text.Length && // yes, checking the length up front is beneficial.
                segment.Equals(_text.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                return _destination;
            }
            else
            {
                return _defaultExit;
            }
        }
    }
}
