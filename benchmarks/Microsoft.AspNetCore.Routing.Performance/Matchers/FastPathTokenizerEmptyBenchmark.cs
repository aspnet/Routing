// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BenchmarkDotNet.Attributes;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    public class FastPathTokenizerEmptyBenchmark
    {
        private const int MaxCount = 32;
        private static readonly string Input = "/";

        // This is super hardcoded implementation for comparison, we dont't expect to do better.
        [Benchmark(Baseline = true)]
        public unsafe void Baseline()
        {
            var segments = stackalloc PathSegment[MaxCount];

            var start = 1;
            var length = Input.Length - start;
            segments[0] = new PathSegment(start, length);
        }

        [Benchmark]
        public unsafe void Implementation()
        {
            var segments = stackalloc PathSegment[MaxCount];

            FastPathTokenizer.Tokenize(Input, segments, MaxCount);
        }
    }
}
