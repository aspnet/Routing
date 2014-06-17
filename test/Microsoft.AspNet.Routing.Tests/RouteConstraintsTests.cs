﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET45

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing.Constraints;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Routing.Tests
{
    public class RouteConstraintsTests
    {
        private const decimal _decimalObject = 2m;

        [Theory]
        [InlineData(42, true)]
        [InlineData("42", true)]
        [InlineData(3.14, false)]
        [InlineData("43.567", false)]
        [InlineData("42a", false)]
        public void IntRouteConstraint_Match_AppliesConstraint(object parameterValue, bool expected)
        {
            // Arrange
            var constraint = new IntRouteConstraint();

            // Act
            var actual = ConstraintsTestHelper.TestConstraint(constraint, parameterValue);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(42, true)]
        [InlineData(42L, true)]
        [InlineData("42", true)]
        [InlineData("9223372036854775807", true)]
        [InlineData(3.14, false)]
        [InlineData("43.567", false)]
        [InlineData("42a", false)]
        public void LongRouteConstraintTests(object parameterValue, bool expected)
        {
            // Arrange
            var constraint = new LongRouteConstraint();

            // Act
            var actual = ConstraintsTestHelper.TestConstraint(constraint, parameterValue);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("alpha", true)]
        [InlineData("a1pha", false)]
        [InlineData("ALPHA", true)]
        [InlineData("A1PHA", false)]
        [InlineData("alPHA", true)]
        [InlineData("A1pHA", false)]
        [InlineData("AlpHA╥", false)]
        [InlineData("", true)]
        public void AlphaRouteConstraintTests(string parameterValue, bool expected)
        {
            // Arrange
            var constraint = new AlphaRouteConstraint();

            // Act
            var actual = ConstraintsTestHelper.TestConstraint(constraint, parameterValue);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(long.MinValue, long.MaxValue, 2, true)]
        [InlineData(3, 5, 3, true)]
        [InlineData(3, 5, 4, true)]
        [InlineData(3, 5, 5, true)]
        [InlineData(3, 5, 6, false)]
        [InlineData(3, 5, 2, false)]
        [InlineData(3, 3, 2, false)]
        [InlineData(3, 3, 3, true)]
        public void RangeRouteConstraintTests(long min, long max, int parameterValue, bool expected)
        {
            // Arrange
            var constraint = new RangeRouteConstraint(min, max);

            // Act
            var actual = ConstraintsTestHelper.TestConstraint(constraint, parameterValue);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void RangeRouteConstraint_MinGreaterThanMax_Throws()
        {
            // Arrange Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new RangeRouteConstraint(3, 2));
            Assert.Equal("The value for argument 'min' should be less than or equal to the value for the argument "+
                         "'max'.\r\nParameter name: min\r\nActual value was 3.",
                         ex.Message);
        }

        [Theory]
        [InlineData(3, 4, true)]
        [InlineData(3, 3, true)]
        [InlineData(3, 2, false)]
        public void MinRouteConstraintTests(long min, int parameterValue, bool expected)
        {
            // Arrange
            var constraint = new MinRouteConstraint(min);

            // Act
            var actual = ConstraintsTestHelper.TestConstraint(constraint, parameterValue);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(3, 2, true)]
        [InlineData(3, 3, true)]
        [InlineData(3, 4, false)]
        public void MaxRouteConstraintTests(long max, int parameterValue, bool expected)
        {
            // Arrange
            var constraint = new MaxRouteConstraint(max);

            // Act
            var actual = ConstraintsTestHelper.TestConstraint(constraint, parameterValue);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(3, "1234", true)]
        [InlineData(3, "123", true)]
        [InlineData(3, "12", false)]
        [InlineData(3, "", false)]
        public void MinLengthRouteConstraintTests(int min, string parameterValue, bool expected)
        {
            // Arrange
            var constraint = new MinLengthRouteConstraint(min);

            // Act
            var actual = ConstraintsTestHelper.TestConstraint(constraint, parameterValue);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MinLengthRouteConstraint_SettingMinLengthLessThanZero_Throws()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new MinLengthRouteConstraint(-1));
            Assert.Equal("Value must be greater than or equal to 0.\r\nParameter name: minLength\r\n" +
                          "Actual value was -1.",
                          ex.Message);
        }

        [Theory]
        [InlineData(3, "", true)]
        [InlineData(3, "12", true)]
        [InlineData(3, "123", true)]
        [InlineData(3, "1234", false)]
        public void MaxLengthRouteConstraintTests(int min, string parameterValue, bool expected)
        {
            // Arrange
            var constraint = new MaxLengthRouteConstraint(min);

            // Act
            var actual = ConstraintsTestHelper.TestConstraint(constraint, parameterValue);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MaxLengthRouteConstraint_SettingMaxLengthLessThanZero_Throws()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(()=> new MaxLengthRouteConstraint(-1));
            Assert.Equal("Value must be greater than or equal to 0.\r\nParameter name: maxLength\r\n" +
                          "Actual value was -1.",
                          ex.Message);
        }

        [Theory]
        [InlineData(3, "123", true)]
        [InlineData(3, "1234", false)]
        public void LengthRouteConstraint_ExactLength_Tests(int length, string parameterValue, bool expected)
        {
            // Arrange
            var constraint = new LengthRouteConstraint(length);

            // Act
            var actual = ConstraintsTestHelper.TestConstraint(constraint, parameterValue);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(3, 5, "12", false)]
        [InlineData(3, 5, "123", true)]
        [InlineData(3, 5, "1234", true)]
        [InlineData(3, 5, "12345", true)]
        [InlineData(3, 5, "123456", false)]
        public void LengthRouteConstraint_Range_Tests(int min, int max, string parameterValue, bool expected)
        {
            // Arrange
            var constraint = new LengthRouteConstraint(min, max);

            // Act
            var actual = ConstraintsTestHelper.TestConstraint(constraint, parameterValue);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LengthRouteConstraint_SettingLengthLessThanZero_Throws()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new LengthRouteConstraint(-1));
            Assert.Equal("Value must be greater than or equal to 0.\r\nParameter name: length\r\n" +
                          "Actual value was -1.",
                          ex.Message);
        }

        [Fact]
        public void LengthRouteConstraint_SettingMinLengthLessThanZero_Throws()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new LengthRouteConstraint(-1, 3));
            Assert.Equal("Value must be greater than or equal to 0.\r\nParameter name: minLength\r\n"+
                         "Actual value was -1.",
                         ex.Message);
        }

        [Fact]
        public void LengthRouteConstraint_SettingMaxLengthLessThanZero_Throws()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new LengthRouteConstraint(0, -1));
            Assert.Equal("Value must be greater than or equal to 0.\r\nParameter name: maxLength\r\n" +
                        "Actual value was -1.",
                        ex.Message);
        }

        [Theory]
        [InlineData("12345678-1234-1234-1234-123456789012", false, true)]
        [InlineData("12345678-1234-1234-1234-123456789012", true, true)]
        [InlineData("12345678901234567890123456789012", false, true)]
        [InlineData("not-parseable-as-guid", false, false)]
        [InlineData(12, false, false)]
        public void GuidRouteConstraintTests(object parameterValue, bool parseBeforeTest, bool expected)
        {
            // Arrange
            if (parseBeforeTest)
            {
                parameterValue = Guid.Parse(parameterValue.ToString());
            }

            var constraint = new GuidRouteConstraint();

            // Act
            var actual = ConstraintsTestHelper.TestConstraint(constraint, parameterValue);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("3.14", true)]
        [InlineData(3.14, true)]
        [InlineData("not-parseable-as-float", false)]
        [InlineData(false, false)]
        [InlineData("1.79769313486232E+300", false)]
        public void FloatRouteConstraintTests(object parameterValue, bool expected)
        {
            // Arrange
            var constraint = new FloatRouteConstraint();

            // Act
            var actual = ConstraintsTestHelper.TestConstraint(constraint, parameterValue);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("3.14", true)]
        [InlineData(3.14f, true)]
        [InlineData(3.14d, true)]
        [InlineData("1.79769313486232E+300", true)]
        [InlineData("not-parseable-as-double", false)]
        [InlineData(false, false)]
        public void DoubleRouteConstraintTests(object parameterValue, bool expected)
        {
            // Arrange
            var constraint = new DoubleRouteConstraint();

            // Act
            var actual = ConstraintsTestHelper.TestConstraint(constraint, parameterValue);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("3.14", true)]
        [InlineData("9223372036854775808.9223372036854775808", true)]
        [InlineData("1.79769313486232E+300", false)]
        [InlineData("not-parseable-as-decimal", false)]
        [InlineData(false, false)]
        [MemberData("GetDecimalObject")]
        public void DecimalRouteConstraintTests(object parameterValue, bool expected)
        {
            // Arrange
            var constraint = new DecimalRouteConstraint();

            // Act
            var actual = ConstraintsTestHelper.TestConstraint(constraint, parameterValue);

            // Assert
            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> GetDateTimeObject
        {
            get
            {
                yield return new object[]
                {
                    DateTime.Now,
                    true
                };
            }
        }

        public static IEnumerable<object[]> GetDecimalObject
        {
            get
            {
                yield return new object[]
                {
                    2m,
                    true
                };
            }
        }

        [Theory]
        [InlineData("12/25/2009", true)]
        [InlineData("25/12/2009 11:45:00 PM", false)]
        [InlineData("12/25/2009 11:45:00 PM", true)]
        [InlineData("11:45:00 PM", true)]
        [InlineData("2009-05-12T11:45:00Z", true)]
        [InlineData("not-parseable-as-date", false)]
        [InlineData(false, false)]
        [MemberData("GetDateTimeObject")]
        public void DateTimeRouteConstraint(object parameterValue, bool expected)
        {
            // Arrange
            var constraint = new DateTimeRouteConstraint();

            // Act
            var actual = ConstraintsTestHelper.TestConstraint(constraint, parameterValue);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("TruE", true)]
        [InlineData("false", true)]
        [InlineData("FalSe", true)]
        [InlineData(" FalSe", true)]
        [InlineData("True ", true)]
        [InlineData(" False ", true)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(1, false)]
        [InlineData("not-parseable-as-bool", false)]
        public void BoolRouteConstraint(object parameterValue, bool expected)
        {
            // Arrange
            var constraint = new BoolRouteConstraint();
            
            // Act
            var actual = ConstraintsTestHelper.TestConstraint(constraint, parameterValue);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public void CompoundRouteConstraint_Match_CallsMatchOnInnerConstraints(bool inner1Result,
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

        private static void AssertMatchWasCalled(Mock<IRouteConstraint> mock, Times times)
        {
            mock.Verify(ConstraintMatchMethodExpression, times);
        }
    }
}

#endif