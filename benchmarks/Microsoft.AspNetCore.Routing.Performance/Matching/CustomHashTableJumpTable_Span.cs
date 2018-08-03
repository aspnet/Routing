// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.AspNetCore.Routing.Matching
{
    internal class CustomHashTableJumpTable_Span : JumpTable
    {
        // Similar to HashHelpers list of primes, but truncated. We don't expect
        // incredibly large numbers to be useful here.
        private static readonly int[] Primes = new int[]
        {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 
            71, 89, 107, 131, 163, 197, 239, 293, 
            353, 431, 521, 631, 761, 919, 1103, 1327, 
            1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 
            7013, 8419, 10103,
        };

        private readonly int _defaultDestination;
        private readonly int _exitDestination;

        private readonly int _prime;
        private readonly int[] _buckets;
        private readonly Entry[] _entries;

        public CustomHashTableJumpTable_Span(
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

            _prime = GetPrime(map.Count);
            _buckets = new int[_prime + 1];
            _entries = new Entry[map.Sum(kvp => kvp.Value.Count)];

            var next = 0;
            foreach (var group in map.GroupBy(kvp => Find(kvp.Key)).OrderBy(g => g.Key))
            {
                _buckets[group.Key] = next;

                foreach (var array in group)
                {
                    for (var i = 0; i < array.Value.Count; i++)
                    {
                        _entries[next++] = new Entry(array.Value[i].text, array.Value[i].destination);
                    }
                }
            }

            Debug.Assert(next == _entries.Length);
            _buckets[_prime] = next;

            var last = 0;
            for (var i = 0; i < _buckets.Length; i++)
            {
                if (_buckets[i] == 0)
                {
                    _buckets[i] = last;
                }
                else
                {
                    last = _buckets[i];
                }
            }
        }

        public int Find(int key)
        {
            return (key & 0x7FFFFFFF) % _prime;
        }

        private static int GetPrime(int capacity)
        {
            for (int i = 0; i < Primes.Length; i++)
            {
                int prime = Primes[i];
                if (prime >= capacity)
                {
                    return prime;
                }
            }

            return Primes[Primes.Length - 1];
        }

        public override int GetDestination(string path, PathSegment segment)
        {
            if (segment.Length == 0)
            {
                return _exitDestination;
            }

            var key = GetKey(path, segment);
            var index = Find(key);

            var start = _buckets[index];
            var end = _buckets[index + 1];

            var entries = _entries.AsSpan(start, end - start);
            for (var i = 0; i < entries.Length; i++)
            {
                var text = entries[i].Text;
                if (text.Length == segment.Length &&
                    string.Compare(
                        path,
                        segment.Start,
                        text,
                        0,
                        segment.Length,
                        StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return entries[i].Destination;
                }
            }

            return _defaultDestination;
        }
        
        private static int GetKey(string path, PathSegment segment)
        {
            return string.GetHashCode(path.AsSpan(segment.Start, segment.Length), StringComparison.OrdinalIgnoreCase);
        }

        private readonly struct Entry
        {
            public readonly string Text;
            public readonly int Destination;

            public Entry(string text, int destination)
            {
                Text = text;
                Destination = destination;
            }
        }
    }
}
