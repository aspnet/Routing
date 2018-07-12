// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal class DictionaryJumpTable : JumpTable
    {
        private readonly int _default;
        private readonly int _exit;
        private readonly Dictionary<string, int> _dictionary;

        public DictionaryJumpTable(int @default, int exit, (string text, int destination)[] entries)
        {
            _default = @default;
            _exit = exit;

            _dictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < entries.Length; i++)
            {
                _dictionary.Add(entries[i].text, entries[i].destination);
            }
        }

        public override int GetDestination(string path, PathSegment segment)
        {
            if (segment.Length == 0)
            {
                return _exit;
            }

            var text = path.Substring(segment.Start, segment.Length);
            if (_dictionary.TryGetValue(text, out var destination))
            {
                return destination;
            }

            return _default;
        }
    }
}
