// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    // Low level implementation of our path tokenization algorithm. Alternative
    // to PathTokenizer.
    internal static class FastPathTokenizer
    {
        public static unsafe int Tokenize(string path, PathSegment* segments, int maxCount)
        {
            int count = 0;
            int start = 1; // Paths always start with a leading /
            int end;
            var span = path.AsSpan(1);
            while ((end = span.IndexOf('/')) >= 0 && count < maxCount)
            {
                segments[count++] = new PathSegment(start, end);
                start += end + 1; // resume search after the current character
                span = path.AsSpan(start);
            }

            // Residue
            var length = span.Length;
            if (length > 0 && count < maxCount)
            {
                segments[count++] = new PathSegment(start, length);
            }

            return count;
        }
    }
}
