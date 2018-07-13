﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    public class SingleEntryJumpTableTest
    {
        [Fact]
        public void GetDestination_ZeroLengthSegment_JumpsToExit()
        {
            // Arrange
            var table = new SingleEntryJumpTable(0, 1, "text", 2);

            // Act
            var result = table.GetDestination("ignored", new PathSegment(0, 0));

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void GetDestination_NonMatchingSegment_JumpsToDefault()
        {
            // Arrange
            var table = new SingleEntryJumpTable(0, 1, "text", 2);

            // Act
            var result = table.GetDestination("text", new PathSegment(1, 2));

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetDestination_SegmentMatchingText_JumpsToDestination()
        {
            // Arrange
            var table = new SingleEntryJumpTable(0, 1, "text", 2);

            // Act
            var result = table.GetDestination("some-text", new PathSegment(5, 4));

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public void GetDestination_SegmentMatchingTextIgnoreCase_JumpsToDestination()
        {
            // Arrange
            var table = new SingleEntryJumpTable(0, 1, "text", 2);

            // Act
            var result = table.GetDestination("some-tExt", new PathSegment(5, 4));

            // Assert
            Assert.Equal(2, result);
        }
    }
}
