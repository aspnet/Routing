// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNET50
using System;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing.Constraints;
using Microsoft.AspNet.Testing;
using Microsoft.Framework.OptionsModel;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Routing
{
    public class RouteConstraintBuilderTest
    {
        [Fact]
        public void AddConstraint_String_CreatesARegex()
        {
            // Arrange
            var builder = CreateBuilder("{controller}/{action}");
            builder.AddConstraint("controller", "abc");

            // Act
            var result = builder.Build();

            // Assert
            Assert.Equal(1, result.Count);
            Assert.Equal("controller", result.First().Key);

            Assert.IsType<RegexRouteConstraint>(Assert.Single(result).Value);
        }

        [Fact]
        public void AddConstraint_IRouteConstraint()
        {
            // Arrange
            var originalConstraint = Mock.Of<IRouteConstraint>();

            var builder = CreateBuilder("{controller}/{action}");
            builder.AddConstraint("controller", originalConstraint);

            // Act
            var result = builder.Build();

            // Assert
            Assert.Equal(1, result.Count);
            Assert.Equal("controller", result.First().Key);

            Assert.Same(originalConstraint, Assert.Single(result).Value);
        }

        [Fact]
        public void AddResolvedConstraint_IRouteConstraint()
        {
            // Arrange
            var builder = CreateBuilder("{controller}/{action}");
            builder.AddResolvedConstraint("controller", "int");

            // Act
            var result = builder.Build();

            // Assert
            Assert.Equal(1, result.Count);
            Assert.Equal("controller", result.First().Key);

            Assert.IsType<IntRouteConstraint>(Assert.Single(result).Value);
        }

        [Fact]
        public void AddConstraint_InvalidType_Throws()
        {
            // Arrange
            var builder = CreateBuilder("{controller}/{action}");

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => builder.AddConstraint("controller", 5),
                "The constraint entry 'controller' - '5' on the route " +
                "'{controller}/{action}' must have a string value or be of a type which implements '" +
                typeof(IRouteConstraint) + "'.");
        }

        [Fact]
        public void AddResolvedConstraint_NotFound_Throws()
        {
            // Arrange
            var unresolvedConstraint = @"test";

            var builder = CreateBuilder("{controller}/{action}");

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => builder.AddResolvedConstraint("controller", unresolvedConstraint),
                @"The constraint entry 'controller' - '" + unresolvedConstraint + "' on the route " +
                "'{controller}/{action}' could not be resolved by the constraint resolver " +
                "of type 'DefaultInlineConstraintResolver'.");
        }

        [Theory]
        [InlineData("abc", "abc", true)]      // simple case
        [InlineData("abc", "bbb|abc", true)]  // Regex or
        [InlineData("Abc", "abc", true)]      // Case insensitive
        [InlineData("Abc ", "abc", false)]    // Matches whole (but no trimming)
        [InlineData("Abcd", "abc", false)]    // Matches whole (additional non whitespace char)
        [InlineData("Abc", " abc", false)]    // Matches whole (less one char)
        public void StringConstraintsMatchingScenarios(string routeValue,
                                                       string constraintValue,
                                                       bool shouldMatch)
        {
            // Arrange
            var routeValues = new RouteValueDictionary(new { controller = routeValue });

            var builder = CreateBuilder("{controller}/{action}");
            builder.AddConstraint("controller", constraintValue);

            var constraint = Assert.Single(builder.Build()).Value;

            Assert.Equal(shouldMatch,
                constraint.Match(
                    httpContext: new Mock<HttpContext>().Object,
                    route: new Mock<IRouter>().Object,
                    routeKey: "controller",
                    values: routeValues,
                    routeDirection: RouteDirection.IncomingRequest));
        }

        private static RouteConstraintBuilder CreateBuilder(string template)
        {
            var services = new Mock<IServiceProvider>(MockBehavior.Strict);

            var options = new Mock<IOptions<RouteOptions>>(MockBehavior.Strict);
            options
                .SetupGet(o => o.Options)
                .Returns(new RouteOptions());

            var inlineConstraintResolver = new DefaultInlineConstraintResolver(services.Object, options.Object);
            return new RouteConstraintBuilder(inlineConstraintResolver, template);
        }
    }
}
#endif
