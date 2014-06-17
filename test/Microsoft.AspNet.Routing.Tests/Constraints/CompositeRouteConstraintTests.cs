﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET45

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.AspNet.Http;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Routing.Tests
{
    public class CompositeRouteConstraintTests
    {
        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public void CompositeRouteConstraint_Match_CallsMatchOnInnerConstraints(bool inner1Result,
                                                                               bool inner2Result,
                                                                               bool expected)
        {
            // Arrange
            var inner1 = MockConstraintWithResult(inner1Result);
            var inner2 = MockConstraintWithResult(inner2Result);

            // Act
            var constraint = new CompositeRouteConstraint(new[] { inner1.Object, inner2.Object });
            var actual = ConstraintsTestHelper.TestConstraint(constraint, null);

            // Assert
            Assert.Equal(expected, actual);
        }

        static Expression<Func<IRouteConstraint, bool>> ConstraintMatchMethodExpression = 
            c => c.Match(It.IsAny<HttpContext>(),
                         It.IsAny<IRouter>(),
                         It.IsAny<string>(),
                         It.IsAny<Dictionary<string, object>>(),
                         It.IsAny<RouteDirection>());

        private static Mock<IRouteConstraint> MockConstraintWithResult(bool result)
        {
            var mock = new Mock<IRouteConstraint>();
            mock.Setup(ConstraintMatchMethodExpression)
                .Returns(result)
                .Verifiable();
            return mock;
        }
    }
}

#endif