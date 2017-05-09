// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.Testing;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Routing.Tests
{
    public class RegexInlineRouteConstraintTests
    {
        [Theory]
        [InlineData("abc", "abc", true)]    // simple match
        [InlineData("Abc", "abc", true)]    // case insensitive match
        [InlineData("Abc ", "abc", false)]   // Extra space on input match (because we don't add ^({0})$
        [InlineData("Abcd", "abc", false)]   // Extra char
        [InlineData("^Abcd", "abc", false)]  // Extra special char
        [InlineData("Abc", " abc", false)]  // Missing char
        [InlineData("123-456-2334", @"\d{3}-\d{3}-\d{4}", true)] // ssn
        [InlineData(@"12/4/2013", @"\d{1,2}\/\d{1,2}\/\d{4}", true)] // date
        [InlineData(@"abc@def.com", @"\w+[\w\.]*\@\w+((-\w+)|(\w*))\.[a-z]{2,3}", true)] // email
        public void RegexInlineConstraintBuildRegexVerbatimFromInput(
            string routeValue,
            string constraintValue,
            bool shouldMatch)
        {
            // Arrange
            var constraint = new RegexInlineRouteConstraint(constraintValue);
            var values = new RouteValueDictionary(new { controller = routeValue });

            // Act
            var match = constraint.Match(
                new DefaultHttpContext(),
                route: new Mock<IRouter>().Object,
                routeKey: "controller",
                values: values,
                routeDirection: RouteDirection.IncomingRequest);

            // Assert
            Assert.Equal(shouldMatch, match);
        }

        [Fact]
        public void RegexInlineConstraint_FailsIfKeyIsNotFoundInRouteValues()
        {
            // Arrange
            var constraint = new RegexInlineRouteConstraint("abc");
            var values = new RouteValueDictionary(new { action = "abc" });

            // Act
            var match = constraint.Match(
                new DefaultHttpContext(),
                route: new Mock<IRouter>().Object,
                routeKey: "controller",
                values: values,
                routeDirection: RouteDirection.IncomingRequest);

            // Assert
            Assert.False(match);
        }

        [Theory]
        [InlineData("tr-TR")]
        [InlineData("en-US")]
        public void RegexInlineConstraint_IsCultureInsensitive(string culture)
        {
            if (TestPlatformHelper.IsMono)
            {
                // The Regex in Mono returns true when matching the Turkish I for the a-z range which causes the test
                // to fail. Tracked via #100.
                return;
            }

            // Arrange
            var constraint = new RegexInlineRouteConstraint("([a-z]+)");
            var values = new RouteValueDictionary(new { controller = "\u0130" }); // Turkish upper-case dotted I

            using (new CultureReplacer(culture))
            {
                // Act
                var match = constraint.Match(
                    new DefaultHttpContext(),
                    route: new Mock<IRouter>().Object,
                    routeKey: "controller",
                    values: values,
                    routeDirection: RouteDirection.IncomingRequest);

                // Assert
                Assert.False(match);
            }
        }
    }
}
