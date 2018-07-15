﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    // Low level implementation of our path tokenization algorithm. Alternative
    // to PathTokenizer.
    internal static class FastPathTokenizer
    {
        // The default limit for the number of segments we tokenize.
        //
        // Historically the limit on the number of segments routing supports is 28.
        // RoutePrecedence computes precedence based on a decimal, which supports 28
        // or 29 digits.
        //
        // So setting this limit to 32 should work pretty well. We also expect the tokenizer
        // to be used with stackalloc, so we want a small number.
        public const int DefaultSegmentCount = 32;

        // This section tokenizes the path by marking the sequence of slashes, and their
        // and the length of the text between them.
        //
        // If there is residue (text after last slash) then the length of the segment will
        // computed based on the string length.
        public static unsafe int Tokenize(string path, PathSegment* segments, int maxCount)
        {
            int count = 0;
            int start = 1; // Paths always start with a leading /
            int end;
            var span = path.AsSpan(start);
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
