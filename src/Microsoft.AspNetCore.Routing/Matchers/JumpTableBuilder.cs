// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal class JumpTableBuilder
    {
        private readonly List<(string text, int destination)> _entries = new List<(string text, int destination)>();

        public int DefaultExit { get; set; }

        public int ZeroExit { get; set; }

        public void AddEntry(string text, int destination)
        {
            _entries.Add((text, destination));
        }

        public JumpTable Build()
        {
            if (_entries.Count == 0)
            {
                return new ZeroEntryJumpTable(ZeroExit, DefaultExit);
            }
            else if (_entries.Count == 1)
            {
                return new SingleEntryJumpTable(_entries[0].text, _entries[0].destination, ZeroExit, DefaultExit);
            }
            else
            {
                return new LinearSearchJumpTable(_entries.ToArray(), ZeroExit, DefaultExit);
            }
        }
    }
}
