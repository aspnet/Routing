// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BenchmarkDotNet.Attributes;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    public class FastPathTokenizerSmallBenchmark
    {
        private const int MaxCount = 32;
        private static readonly string Input = "/hello/world/cool";

        // This is a naive reference implementation. We expect to do better.
        [Benchmark(Baseline = true)]
        public unsafe void Baseline()
        {
            var path = Input;
            var segments = stackalloc PathSegment[MaxCount];

            int count = 0;
            int start = 1; // Paths always start with a leading /
            int end;
            while ((end = path.IndexOf('/', start)) >= 0 && count < MaxCount)
            {
                segments[count++] = new PathSegment(start, end - start);
                start = end + 1; // resume search after the current character
            }

            // Residue
            var length = path.Length - start;
            if (length > 0 && count < MaxCount)
            {
                segments[count++] = new PathSegment(start, length);
            }
        }

        [Benchmark]
        public unsafe void Implementation()
        {
            var segments = stackalloc PathSegment[MaxCount];

            FastPathTokenizer.Tokenize(Input, segments, MaxCount);
        }
    }
}
