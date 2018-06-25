// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using BenchmarkDotNet.Attributes;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    public class JumpTableSingleEntryBenchmark
    {
        private JumpTable _table;
        private string[] _strings;
        private PathSegment[] _segments;

        [GlobalSetup]
        public void Setup()
        {
            _table = new SingleEntryJumpTable(0, -1, "hello-world", 1);
            _strings = new string[]
            {
                "index/foo/2",
                "index/hello-world1/2",
                "index/hello-world/2",
                "index//2",
                "index/hillo-goodbye/2",
            };
            _segments = new PathSegment[]
            {
                new PathSegment(6, 3),
                new PathSegment(6, 12),
                new PathSegment(6, 10),
                new PathSegment(6, 0),
                new PathSegment(6, 13),
            };
        }

        [Benchmark(Baseline = true, OperationsPerInvoke = 5)]
        public int Baseline()
        {
            var strings = _strings;
            var segments = _segments;

            var index = 0;
            for (var i = 0; i < strings.Length; i++)
            {
                index = segments[i].Length == 0 ? -1 :
                    segments[i].Length != 11 ? 1 :
                    string.Compare(
                        strings[i],
                        segments[i].Start,
                        "hello-world",
                        0,
                        11,
                        StringComparison.OrdinalIgnoreCase);
            }

            return index;
        }

        [Benchmark(OperationsPerInvoke = 5)]
        public int Implementation()
        {
            var strings = _strings;
            var segments = _segments;

            var index = 0;
            for (var i = 0; i < strings.Length; i++)
            {
                index = _table.GetDestination(strings[i], segments[i]);
            }

            return index;
        }
    }
}
