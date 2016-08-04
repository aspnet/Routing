// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Constraints;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Routing.Tests.Constraints
{
    public class StringRouteConstraintTest
    {
        [Fact]
        public void StringRouteConstraintSimpleTrueTest()
        {
            var constraint = new StringRouteConstraint("home");
            var values = new RouteValueDictionary(new { controller = "home" });

            var match = constraint.Match(
              httpContext: Mock.Of<HttpContext>(),
              route: new Mock<IRouter>().Object,
              routeKey: "controller",
              values: values,
              routeDirection: RouteDirection.IncomingRequest);

            // Assert
            Assert.True(match);
        }

        [Fact]
        public void StringRouteConstraintKeyNotFoundTest()
        {
            var constraint = new StringRouteConstraint("admin");
            var values = new RouteValueDictionary(new { controller = "admin" });

            var match = constraint.Match(
                httpContext: Mock.Of<HttpContext>(),
                route: new Mock<IRouter>().Object,
                routeKey: "action",
                values: values,
                routeDirection: RouteDirection.IncomingRequest);

            // Assert
            Assert.False(match);
        }

        [Theory]
        [InlineData("User", "uSer", true)]
        [InlineData("User.Admin", "User.Admin", true)]
        [InlineData(@"User\Admin", "User\\Admin", true)]
        public void StringRouteConstraintEscapingAndCaseSensitiveTest(string routeValue, string constraintValue, bool expected)
        {
            var constraint = new StringRouteConstraint(constraintValue);
            var values = new RouteValueDictionary(new { controller = routeValue });

            var match = constraint.Match(
              httpContext: Mock.Of<HttpContext>(),
              route: new Mock<IRouter>().Object,
              routeKey: "controller",
              values: values,
              routeDirection: RouteDirection.IncomingRequest);

            // Assert
            Assert.Equal(expected, match);
        }
    }
}