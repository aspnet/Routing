// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal class JumpTableBuilder
    {
        private readonly List<(string text, int destination)> _entries = new List<(string text, int destination)>();

        // The destination state when none of the text entries match.
        public int Default { get; set; } = -1;

        // The destination state for a zero-length segment. This is a special
        // case because parameters don't match a zero-length segment.
        public int Exit { get; set; } = -1;

        public void AddEntry(string text, int destination)
        {
            _entries.Add((text, destination));
        }

        public JumpTable Build()
        {
            if (_entries.Count == 0)
            {
                return new ZeroEntryJumpTable(Default, Exit);
            }

            if (_entries.Count == 1)
            {
                var entry = _entries[0];
                return new SingleEntryJumpTable(Default, Exit, entry.text, entry.destination);
            }

            if (_entries.Count < 10)
            {
                return new LinearSearchJumpTable(Default, Exit, _entries.ToArray());
            }

            return new DictionaryJumpTable(Default, Exit, _entries.ToArray());
        }
    }
}
