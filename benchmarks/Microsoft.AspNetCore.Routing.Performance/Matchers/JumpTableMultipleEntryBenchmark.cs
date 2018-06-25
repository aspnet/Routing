// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    public class JumpTableMultipleEntryBenchmark
    {
        private string[] _strings;
        private PathSegment[] _segments;
        private int[] _samples;

        private JumpTable _linearSearch;
        private JumpTable _asciiKeyed;
        private JumpTable _dictionary;
        private JumpTable _customDictionary;

        // All factors of 100 to support sampling
        [Params(2, 5, 10, 25, 50, 100)]
        public int Count;

        [GlobalSetup]
        public void Setup()
        {
            _strings = GetStrings(100);
            _segments = new PathSegment[100];

            for (var i = 0; i < _strings.Length; i++)
            {
                _segments[i] = new PathSegment(0, _strings[i].Length);
            }

            _samples = new int[Count];
            for (var i = 0; i < _samples.Length; i++)
            {
                _samples[i] = i * (_strings.Length / Count);
            }

            var entries = new List<(string text, int _)>();
            for (var i = 0; i < _samples.Length; i++)
            {
                entries.Add((_strings[_samples[i]], i));
            }

            _linearSearch = new LinearSearchJumpTable(0, -1, entries.ToArray());
            
            if (!AsciiKeyedJumpTable.TryCreate(0, -1, entries, out _asciiKeyed))
            {
                throw new InvalidOperationException("Ooops, something went wrong.");
            }

            _dictionary = new DictionaryJumpTable(0, -1, entries.ToArray());
            _customDictionary = new CustomDictionaryJumpTable(0, -1, entries.ToArray());
        }

        // This baseline is similar to SingleEntryJumpTable. We just want
        // something stable to compare against.
        [Benchmark(Baseline = true, OperationsPerInvoke = 100)]
        public int Baseline()
        {
            var strings = _strings;
            var segments = _segments;

            var index = 0;
            for (var i = 0; i < strings.Length; i++)
            {
                index = segments[i].Length == 0 ? -1 :
                    segments[i].Length != strings[i].Length ? 1 :
                    string.Compare(
                        strings[i],
                        segments[i].Start,
                        strings[i],
                        0,
                        strings[i].Length,
                        StringComparison.OrdinalIgnoreCase);
            }

            return index;
        }

        [Benchmark(OperationsPerInvoke = 100)]
        public int LinearSearch()
        {
            var strings = _strings;
            var segments = _segments;
            var samples = _samples;

            var index = 0;
            for (var i = 0; i < strings.Length; i++)
            {
                index = _linearSearch.GetDestination(strings[i], segments[i]);
            }

            return index;
        }

        [Benchmark(OperationsPerInvoke = 100)]
        public int AsciiKeyed()
        {
            var strings = _strings;
            var segments = _segments;
            var samples = _samples;

            var index = 0;
            for (var i = 0; i < strings.Length; i++)
            {
                index = _asciiKeyed.GetDestination(strings[i], segments[i]);
            }

            return index;
        }

        [Benchmark(OperationsPerInvoke = 100)]
        public int Dictionary()
        {
            var strings = _strings;
            var segments = _segments;
            var samples = _samples;

            var index = 0;
            for (var i = 0; i < strings.Length; i++)
            {
                index = _dictionary.GetDestination(strings[i], segments[i]);
            }

            return index;
        }

        [Benchmark(OperationsPerInvoke = 100)]
        public int CustomDictionar11y()
        {
            var strings = _strings;
            var segments = _segments;
            var samples = _samples;

            var index = 0;
            for (var i = 0; i < strings.Length; i++)
            {
                index = _customDictionary.GetDestination(strings[i], segments[i]);
            }

            return index;
        }

        private static string[] GetStrings(int count)
        {
            var strings = new string[count];
            for (var i = 0; i < count; i++)
            {
                var guid = Guid.NewGuid().ToString();

                // Between 5 and 36 characters
                var text = guid.Substring(0, Math.Max(5, Math.Min(count, 36)));
                if (char.IsDigit(text[0]))
                {
                    // Convert first character to a letter.
                    text = ((char)(text[0] + ('G' - '0'))) + text.Substring(1);
                }

                if (i % 2 == 0)
                {
                    // Lowercase half of them
                    text = text.ToLowerInvariant();
                }

                strings[i] = text;
            }

            return strings;
        }

        private class DictionaryJumpTable : JumpTable
        {
            private readonly int _default;
            private readonly int _exit;
            private readonly Dictionary<int, (string text, int destination)[]> _store;

            public DictionaryJumpTable(int @default, int exit, (string text, int destination)[] entries)
            {
                _default = @default;
                _exit = exit;

                var map = new Dictionary<int, List<(string text, int destination)>>();

                for (var i = 0; i < entries.Length; i++)
                {
                    var key = GetKey(entries[i].text.AsSpan());
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
                    return _exit;
                }

                var key = GetKey(path.AsSpan(segment.Start, segment.Length));
                if (_store.TryGetValue(key, out var entries))
                {
                    for (var i = 0; i < entries.Length; i++)
                    {
                        if (entries[i].text.Length == segment.Length &&
                            string.Compare(
                                path,
                                segment.Start,
                                entries[i].text,
                                0,
                                segment.Length,
                                StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return entries[i].destination;
                        }
                    }
                }

                return _default;
            }

            private static int GetKey(string path, PathSegment segment)
            {
                return GetKey(path.AsSpan(segment.Start, segment.Length));
            }

            private static int GetKey(ReadOnlySpan<char> span)
            {
                var length = (byte)(span.Length & 0xFF);

                byte c0, c1, c2;
                switch (length)
                {
                    case 0:
                        {
                            return 0;
                        }

                    case 1:
                        {
                            c0 = (byte)(span[0] & 0x5F);
                            return (length << 24) | (c0 << 16);
                        }

                    case 2:
                        {
                            c0 = (byte)(span[0] & 0x5F);
                            c1 = (byte)(span[1] & 0x5F);
                            return (length << 24) | (c0 << 16) | (c1 << 8);
                        }

                    default:
                        {
                            c0 = (byte)(span[0] & 0x5F);
                            c1 = (byte)(span[1] & 0x5F);
                            c2 = (byte)(span[2] & 0x5F);
                            return (length << 24) | (c0 << 16) | (c1 << 8) | c2;
                        }
                }
            }
        }

        private class CustomDictionaryJumpTable : JumpTable
        {
            // Similar to HashHelpers list of primes, but truncated. We don't expect
            // incredibly large numbers to be useful here.
            private static readonly int[] Primes = new int[]
            {
                3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
                1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103,
            };
            
            private readonly int _default;
            private readonly int _exit;

            private readonly int _prime;
            private readonly int[] _buckets;
            private readonly Entry[] _entries;

            public CustomDictionaryJumpTable(int @default, int exit, (string text, int destination)[] entries)
            {
                _default = @default;
                _exit = exit;

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
                foreach (var group in map.GroupBy(kvp => kvp.Key % _prime).OrderBy(g => g.Key))
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
                return key % _prime;
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

            public unsafe override int GetDestination(string path, PathSegment segment)
            {
                if (segment.Length == 0)
                {
                    return _exit;
                }
                
                var key = GetKey(path, segment);
                var index = Find(key);
                
                var start = _buckets[index];
                var end = _buckets[index + 1];

                var span = _entries.AsSpan(start, end - start);
                for (var i = 0; i < span.Length; i++)
                {
                    ref var entry = ref span[i];
                    if (entry.Text.Length == segment.Length &&
                        string.Compare(
                            path,
                            segment.Start,
                            entry.Text,
                            0,
                            segment.Length,
                            StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return entry.Destination;
                    }
                }

                return _default;
            }

            private unsafe static int GetKey(string path, PathSegment segment)
            {
                fixed (char* p = path)
                {
                    switch (path.Length)
                    {
                        case 0:
                            {
                                return 0;
                            }

                        case 1:
                            {
                                return
                                    ((*(p + segment.Start + 0) & 0x5F) << (0 * 8));
                            }

                        case 2:
                            {
                                return
                                    ((*(p + segment.Start + 0) & 0x5F) << (0 * 8)) |
                                    ((*(p + segment.Start + 1) & 0x5F) << (1 * 8));
                            }

                        case 3:
                            {
                                return
                                    ((*(p + segment.Start + 0) & 0x5F) << (0 * 8)) |
                                    ((*(p + segment.Start + 1) & 0x5F) << (1 * 8)) |
                                    ((*(p + segment.Start + 2) & 0x5F) << (2 * 8));
                            }

                        default:
                            {
                                return
                                    ((*(p + segment.Start + 0) & 0x5F) << (0 * 8)) |
                                    ((*(p + segment.Start + 1) & 0x5F) << (1 * 8)) |
                                    ((*(p + segment.Start + 2) & 0x5F) << (2 * 8)) |
                                    ((*(p + segment.Start + 3) & 0x5F) << (3 * 8));
                            }
                    }
                }
            }
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
