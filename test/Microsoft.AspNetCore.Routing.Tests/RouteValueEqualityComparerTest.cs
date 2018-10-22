﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.AspNetCore.Routing
{
    public class RouteValueEqualityComparerTest
    {
        private readonly RouteValueEqualityComparer _comparer;

        public RouteValueEqualityComparerTest()
        {
            _comparer = new RouteValueEqualityComparer();
        }

        [Theory]
        [InlineData(5, 7, false)]
        [InlineData("foo", "foo", true)]
        [InlineData("foo", "FoO", true)]
        [InlineData("foo", "boo", false)]
        [InlineData("7", 7, true)]
        [InlineData(7, "7", true)]
        [InlineData(5.7d, 5.7d, true)]
        [InlineData(null, null, true)]
        [InlineData(null, "foo", false)]
        [InlineData("foo", null, false)]
        [InlineData(null, "", true)]
        [InlineData("", null, true)]
        [InlineData("", "", true)]
        [InlineData("", "foo", false)]
        [InlineData("foo", "", false)]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        public void EqualsTest(object x, object y, bool expected)
        {
            var actual = _comparer.Equals(x, y);
            Assert.Equal(expected, actual);
        }
    }
}
