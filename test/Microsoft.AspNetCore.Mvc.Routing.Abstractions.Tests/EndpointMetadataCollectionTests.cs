﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Microsoft.AspNetCore.Routing
{
    public class EndpointMetadataCollectionTests
    {
        [Fact]
        public void Constructor_Enumeration_ContainsValues()
        {
            // Arrange & Act
            var metadata = new EndpointMetadataCollection(new List<object>
            {
                1,
                2,
                3,
            });

            // Assert
            Assert.Equal(3, metadata.Count);

            Assert.Collection(metadata,
                value => Assert.Equal(1, value),
                value => Assert.Equal(2, value),
                value => Assert.Equal(3, value));
        }

        [Fact]
        public void Constructor_ParamsArray_ContainsValues()
        {
            // Arrange & Act
            var metadata = new EndpointMetadataCollection(1, 2, 3);

            // Assert
            Assert.Equal(3, metadata.Count);

            Assert.Collection(metadata,
                value => Assert.Equal(1, value),
                value => Assert.Equal(2, value),
                value => Assert.Equal(3, value));
        }
    }
}