// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BenchmarkDotNet.Attributes;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    public class FastPathTokenizerLargeBenchmark
    {
        private static readonly int MaxCount = 32;
        private static readonly string Input = 
            "/heeeeeeeeeeyyyyyyyyyyy/this/is/a/string/with/lots/of/segments" +
            "/hoooooooooooooooooooooooooooooooooow long/do you think it should be?/I think" +
            "/like/32/segments/is /a/goood/number/dklfl/20303/dlflkf" +
            "/Im/tired/of/thinking/of/more/things/to/so";

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
