// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.Encodings.Web;
using Xunit;

namespace Microsoft.AspNetCore.Routing.Internal
{
    // Note that these tests look a little wierd. It's because each test treats the input
    // as a single route segment that *might* contain slashes. While we're producing paths, these
    // are unit tests for the encoding functionality.
    public class UriBuildingContextTest
    {
        [Fact]
        public void EncodePathValue_EncodesEntireValue_WhenEncodeSlashes_IsFalse()
        {
            // Arrange
            var value = "a/b b1/c";
            var expected = "/a%2Fb%20b1%2Fc";
            var uriBuilldingContext = new UriBuildingContext(UrlEncoder.Default);
            
            // Act
            uriBuilldingContext.EncodePathValue(value, 0, value.Length, encodeSlashes: true);

            // Assert
            Assert.Equal(expected, uriBuilldingContext.ToString());
        }

        [Fact]
        public void EncodePathValue_EncodesOnlySlashes_WhenEncodeSlashes_IsFalse()
        {
            // Arrange
            var value = "a/b b1/c";
            var expected = "/a/b%20b1/c";
            var uriBuilldingContext = new UriBuildingContext(UrlEncoder.Default);

            // Act
            uriBuilldingContext.EncodePathValue(value, 0, value.Length, encodeSlashes: false);

            // Assert
            Assert.Equal(expected, uriBuilldingContext.ToString());
        }

        [Theory]
        [InlineData("a/b b1/c", 0, 2, "/a/")]
        [InlineData("a/b b1/c", 3, 4, "/%20b1/")]
        [InlineData("a/b b1/c", 3, 5, "/%20b1/c")]
        [InlineData("a/b b1/c/", 8, 1, "/")]
        [InlineData("/", 0, 1, "/")]
        [InlineData("/a", 0, 2, "/a")]
        [InlineData("a", 0, 1, "/a")]
        [InlineData("a/", 0, 2, "/a/")]
        public void EncodePathValue_EncodesOnlySlashes_WithinSubsegment_WhenEncodeSlashes_IsFalse(
            string value,
            int startIndex,
            int characterCount,
            string expected)
        {
            // Arrange
            var uriBuilldingContext = new UriBuildingContext(UrlEncoder.Default);

            // Act
            uriBuilldingContext.EncodePathValue(value, startIndex, characterCount, encodeSlashes: false);

            // Assert
            Assert.Equal(expected, uriBuilldingContext.ToString());
        }
        
        [Theory]
        [InlineData("/Author", false, false, "/Author")]
        [InlineData("/Author", false, true, "/Author")]
        [InlineData("/Author", true, false, "/Author/")]
        [InlineData("/Author", true, true, "/Author/")]
        [InlineData("/Author/", false, false, "/Author/")]
        [InlineData("/Author/", false, true, "/Author%2F")]
        [InlineData("/Author/", true, false, "/Author/")]
        [InlineData("/Author/", true, true, "/Author%2F/")]
        [InlineData("Author", false, false, "/Author")]
        [InlineData("Author", false, true, "/Author")]
        [InlineData("Author", true, false, "/Author/")]
        [InlineData("Author", true, true, "/Author/")]
        [InlineData("", false, false, "")]
        [InlineData("", false, true, "")]
        [InlineData("", true, false, "")]
        [InlineData("", true, true, "")]
        public void ToPathString_EncodesSlashesInsideSegment(string url, bool appendTrailingSlash, bool encodeSlashes, string expected)
        {
            // Arrange
            var uriBuilldingContext = new UriBuildingContext(UrlEncoder.Default);
            uriBuilldingContext.AppendTrailingSlash = appendTrailingSlash;

            // Act
            uriBuilldingContext.Accept(url, encodeSlashes);

            // Assert
            Assert.Equal(expected, uriBuilldingContext.ToPathString().Value);
        }
    }
}
