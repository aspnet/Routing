// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET45

using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing.Constraints;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Routing.Tests
{
    public class RegexConstraintTests
    {
        [Theory]
        [InlineData("abc", "abc", true)]    // simple match
        [InlineData("Abc", "abc", true)]    // case insensitive match
        [InlineData("Abc ", "abc", true)]   // Extra space on input match (because we don't add ^({0})$
        [InlineData("Abcd", "abc", true)]   // Extra char
        [InlineData("^Abcd", "abc", true)]  // Extra special char
        [InlineData("Abc", " abc", false)]  // Missing char
        public void RegexConstraintBuildRegexVerbatimFromInput(string routeValue,
                                                               string constraintValue,
                                                               bool shouldMatch)
        {
            // Arrange
            var constraint = new RegexRouteConstraint(constraintValue);
            var values = new RouteValueDictionary(new {controller = routeValue});

            // Assert
            Assert.Equal(shouldMatch, EasyMatch(constraint, "controller", values));
        }

        [Fact]
        public void RegexConstraint_TakesRegexAsInput_SimpleMatch()
        {
            // Arrange
            var constraint = new RegexRouteConstraint(new Regex("^abc$"));
            var values = new RouteValueDictionary(new { controller = "abc"});

            // Assert
            Assert.True(EasyMatch(constraint, "controller", values));
        }

        [Fact]
        public void RegexConstraintConstructedWithRegex_SimpleFailedMatch()
        {
            // Arrange
            var constraint = new RegexRouteConstraint(new Regex("^abc$"));
            var values = new RouteValueDictionary(new { controller = "Abc" });

            // Assert
            Assert.False(EasyMatch(constraint, "controller", values));
        }

        [Fact]
        public void RegexConstraintFailsIfKeyIsNotFoundInRouteValues()
        {
            // Arrange
            var constraint = new RegexRouteConstraint(new Regex("^abc$"));
            var values = new RouteValueDictionary(new { action = "abc" });

            // Assert
            Assert.False(EasyMatch(constraint, "controller", values));
        }

        [Fact]
        public void RegexConstraintIsCultureInsensitiveWhenConstructredWithString()
        {
            // Arrange
            var constraint = new RegexRouteConstraint("^([a-z]+)$");
            var values = new RouteValueDictionary(new { controller = "\u0130" }); // Turkish upper-case dotted I

            var currentThread = Thread.CurrentThread;
            var backupCulture = currentThread.CurrentCulture;

            bool matchInTurkish;
            bool matchInUsEnglish;

            // Act
            try
            {
                currentThread.CurrentCulture = new CultureInfo("tr-TR"); // Turkish culture
                matchInTurkish = EasyMatch(constraint, "controller", values);

                currentThread.CurrentCulture = new CultureInfo("en-US");
                matchInUsEnglish = EasyMatch(constraint, "controller", values);
            }
            finally
            {
                currentThread.CurrentCulture = backupCulture;
            }

            // Assert
            Assert.False(matchInUsEnglish); // this just verifies the test
            Assert.False(matchInTurkish);
        }

        private static bool EasyMatch(IRouteConstraint constraint,
                                      string routeKey,
                                      RouteValueDictionary values)
        {
            return constraint.Match(httpContext: new Mock<HttpContext>().Object,
                route: new Mock<IRouter>().Object,
                routeKey: routeKey,
                values: values,
                routeDirection: RouteDirection.IncomingRequest);
        }
    }
}
#endif