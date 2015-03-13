// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if DNX451
using Moq;
using Xunit;

namespace Microsoft.AspNet.Routing
{
    public class VirtualPathDataTests
    {
        [Fact]
        public void Constructor_CreatesEmptyDataTokensIfNull()
        {
            // Arrange
            var router = new Mock<IRouter>().Object;
            var path = "virtual path";

            // Act
            var pathData1 = new VirtualPathData(router, path);
            var pathData2 = new VirtualPathData(router, path, null);

            // Assert
            Assert.Same(router, pathData1.Router);
            Assert.Same(path, pathData1.VirtualPath);
            Assert.NotNull(pathData1.DataTokens);
            Assert.Empty(pathData1.DataTokens);

            Assert.Same(router, pathData2.Router);
            Assert.Same(path, pathData2.VirtualPath);
            Assert.NotNull(pathData2.DataTokens);
            Assert.Empty(pathData2.DataTokens);
        }

        [Fact]
        public void Constructor_SetDataTokens()
        {
            // Arrange
            var router = new Mock<IRouter>().Object;
            var path = "virtual path";
            var dataTokens = new RouteValueDictionary();
            dataTokens["TestKey"] = "TestValue";

            // Act
            var pathData = new VirtualPathData(router, path, dataTokens);

            // Assert
            Assert.Same(router, pathData.Router);
            Assert.Same(path, pathData.VirtualPath);
            Assert.NotNull(pathData.DataTokens);
            Assert.Equal("TestValue", pathData.DataTokens["TestKey"]);
            Assert.Same(dataTokens, pathData.DataTokens);
        }

        [Fact]
        public void VirtualPath_ReturnsEmptyStringIfNull()
        {
            // Arrange
            var router = new Mock<IRouter>().Object;

            // Act
            var pathData = new VirtualPathData(router, virtualPath: null);

            // Assert
            Assert.Same(router, pathData.Router);
            Assert.Equal(string.Empty, pathData.VirtualPath);
            Assert.NotNull(pathData.DataTokens);
            Assert.Empty(pathData.DataTokens);
        }
    }
}
#endif