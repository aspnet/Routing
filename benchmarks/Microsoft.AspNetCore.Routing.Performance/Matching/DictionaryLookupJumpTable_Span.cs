// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Routing.Matching
{
    internal class DictionaryLookupJumpTable_Span : JumpTable
    {
        private readonly int _defaultDestination;
        private readonly int _exitDestination;
        private readonly Dictionary<int, (string text, int destination)[]> _store;

        public DictionaryLookupJumpTable_Span(
            int defaultDestination,
            int exitDestination,
            (string text, int destination)[] entries)
        {
            _defaultDestination = defaultDestination;
            _exitDestination = exitDestination;

            var map = new Dictionary<int, List<(string text, int destination)>>();

            for (var i = 0; i < entries.Length; i++)
            {
                var key = GetKey(entries[i].text, new PathSegment(0, entries[i].text.Length));
                if (!map.TryGetValue(key, out var matches))
                {
                    matches = new List<(string text, int destination)>();
                    map.Add(key, matches);
                }

                matches.Add(entries[i]);
            }

            _store = map.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
        }

        public override int GetDestination(string path, PathSegment segment)
        {
            if (segment.Length == 0)
            {
                return _exitDestination;
            }

            var key = GetKey(path, segment);
            if (_store.TryGetValue(key, out var entries))
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    var text = entries[i].text;
                    if (text.Length == segment.Length &&
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
            }

            return _defaultDestination;
        }

        private static int GetKey(string path, PathSegment segment)
        {
            return string.GetHashCode(path.AsSpan(segment.Start, segment.Length), StringComparison.OrdinalIgnoreCase);
        }
    }
}
