// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing.Constraints;
using Microsoft.AspNet.Testing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Routing.Tests
{
    public class RegexInlineRouteConstraintTests
    {
        [Theory]
        [InlineData("abc", "abc", true)]    // simple match
        [InlineData("Abc", "abc", true)]    // case insensitive match
        [InlineData("Abc ", "abc", true)]   // Extra space on input match (because we don't add ^({0})$
        [InlineData("Abcd", "abc", true)]   // Extra char
        [InlineData("^Abcd", "abc", true)]  // Extra special char
        [InlineData("Abc", " abc", false)]  // Missing char
        public void RegexInlineConstraintBuildRegexVerbatimFromInput(
            string routeValue,
            string constraintValue,
            bool shouldMatch)
        {
            // Arrange
            var constraint = new RegexInlineRouteConstraint(constraintValue);
            var values = new RouteValueDictionary(new {controller = routeValue});

            // Assert
            Assert.Equal(shouldMatch, EasyMatch(constraint, "controller", values));
        }

        [Fact]
        public void RegexInlineConstraint_FailsIfKeyIsNotFoundInRouteValues()
        {
            // Arrange
            var constraint = new RegexInlineRouteConstraint("^abc$");
            var values = new RouteValueDictionary(new { action = "abc" });

            // Assert
            Assert.False(EasyMatch(constraint, "controller", values));
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
            var constraint = new RegexInlineRouteConstraint("^([a-z]+)$");
            var values = new RouteValueDictionary(new { controller = "\u0130" }); // Turkish upper-case dotted I

            using (new CultureReplacer(culture))
            {
                // Act
                var match = EasyMatch(constraint, "controller", values);

                // Assert
                Assert.False(match);
            }
        }

        private static bool EasyMatch(
            IRouteConstraint constraint,
            string routeKey,
            RouteValueDictionary values)
        {
            return constraint.Match(
                httpContext: new Mock<HttpContext>().Object,
                route: new Mock<IRouter>().Object,
                routeKey: routeKey,
                values: values,
                routeDirection: RouteDirection.IncomingRequest);
        }
    }
}
