// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal class JumpTableBuilder
    {
        private readonly List<(string text, int destination)> _entries = new List<(string text, int destination)>();

        public int Default { get; set; }

        public int Exit { get; set; }

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

            if (_entries.Count > 4 && AsciiKeyedJumpTable.TryCreate(Default, Exit, _entries, out var result))
            {
                return result;
            }

            return new LinearSearchJumpTable(Default, Exit, _entries.ToArray());
        }
    }
}
