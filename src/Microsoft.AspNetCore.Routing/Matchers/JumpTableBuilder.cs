// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal class JumpTableBuilder
    {
        private readonly List<(string text, int destination)> _entries = new List<(string text, int destination)>();

        // The destination state when none of the text entries match.
        public int DefaultDestination { get; set; } = -1;

        // The destination state for a zero-length segment. This is a special
        // case because parameters don't match a zero-length segment.
        public int ExitDestination { get; set; } = -1;

        public void AddEntry(string text, int destination)
        {
            _entries.Add((text, destination));
        }

        public JumpTable Build()
        {
            if (DefaultDestination < 0)
            {
                throw new InvalidOperation($"{nameof(DefaultDestination)} is not set. This is a bug.");
            }

            if (ExitDestination < 0)
            {
                throw new InvalidOperation($"{nameof(ExitDestination)} is not set. This is a bug.");
            }

            if (_entries.Count == 0)
            {
                return new ZeroEntryJumpTable(DefaultDestination, ExitDestination);
            }

            if (_entries.Count == 1)
            {
                var entry = _entries[0];
                return new SingleEntryJumpTable(DefaultDestination, ExitDestination, entry.text, entry.destination);
            }

            if (_entries.Count < 10)
            {
                return new LinearSearchJumpTable(DefaultDestination, ExitDestination, _entries.ToArray());
            }

            return new DictionaryJumpTable(DefaultDestination, ExitDestination, _entries.ToArray());
        }
    }
}
