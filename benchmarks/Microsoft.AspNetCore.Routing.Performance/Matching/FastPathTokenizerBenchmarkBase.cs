﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Routing.Matching
{
    public abstract class FastPathTokenizerBenchmarkBase
    {
        internal unsafe void NaiveBaseline(string path, PathSegment* segments, int maxCount)
        {
            int count = 0;
            int start = 1; // Paths always start with a leading /
            int end;
            while ((end = path.IndexOf('/', start)) >= 0 && count < maxCount)
            {
                segments[count++] = new PathSegment(start, end - start);
                start = end + 1; // resume search after the current character
            }

            // Residue
            var length = path.Length - start;
            if (length > 0 && count < maxCount)
            {
                segments[count++] = new PathSegment(start, length);
            }
        }

        internal unsafe void MinimalBaseline(string path, PathSegment* segments, int maxCount)
        {
            var start = 1;
            var length = path.Length - start;
            segments[0] = new PathSegment(start, length);
        }
    }
}
