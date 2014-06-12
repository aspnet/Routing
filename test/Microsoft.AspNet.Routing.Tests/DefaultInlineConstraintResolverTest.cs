﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET45
using System;
using System.Collections.Generic;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing.Constraints;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Routing.Tests
{
    public class DefaultInlineConstraintResolverTest
    {
        private IInlineConstraintResolver _constraintResolver;

        public DefaultInlineConstraintResolverTest()
        {
            var routeOptions = new RouteOptions();
            _constraintResolver = GetInlineConstraintResolver(routeOptions);
        }
        [Fact]
        public void ResolveConstraint_IntConstraint_ResolvesCorrectly()
        {
            // Arrange & Act
            var constraint = _constraintResolver.ResolveConstraint("int");

            // Assert
            Assert.IsType<IntRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_IntConstraintWithArgument_Throws()
        {
            // Arrange, Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(
                () => _constraintResolver.ResolveConstraint("int(5)"));
            Assert.Equal("Could not find a constructor for constraint type 'IntRouteConstraint'"+
                         " with the following number of parameters: 1.",
                         ex.Message);
        }
        [Fact]
        public void ResolveConstraint_AlphaConstraint()
        {
            // Arrange & Act
            var constraint = _constraintResolver.ResolveConstraint("alpha");

            // Assert
            Assert.IsType<AlphaRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_BoolConstraint()
        {
            // Arrange & Act
            var constraint = _constraintResolver.ResolveConstraint("bool");

            // Assert
            Assert.IsType<BoolRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_CompositeConstraintIsNotRegistered()
        {
            // Arrange, Act & Assert
            Assert.Null(_constraintResolver.ResolveConstraint("composite"));
        }

        [Fact]
        public void ResolveConstraint_DateTimeConstraint()
        {
            // Arrange & Act
            var constraint = _constraintResolver.ResolveConstraint("datetime");

            // Assert
            Assert.IsType<DateTimeRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_DecimalConstraint()
        {
            // Arrange & Act
            var constraint = _constraintResolver.ResolveConstraint("decimal");

            // Assert
            Assert.IsType<DecimalRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_DoubleConstraint()
        {
            // Arrange & Act
            var constraint = _constraintResolver.ResolveConstraint("double");

            // Assert
            Assert.IsType<DoubleRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_FloatConstraint()
        {
            // Arrange & Act
            var constraint = _constraintResolver.ResolveConstraint("float");

            // Assert
            Assert.IsType<FloatRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_GuidConstraint()
        {
            // Arrange & Act
            var constraint = _constraintResolver.ResolveConstraint("guid");

            // Assert
            Assert.IsType<GuidRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_IntConstraint()
        {
            // Arrange & Act
            var constraint = _constraintResolver.ResolveConstraint("int");

            // Assert
            Assert.IsType<IntRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_LengthConstraint()
        {
            // Arrange & Act
            var constraint = _constraintResolver.ResolveConstraint("length(5)");

            // Assert
            Assert.IsType<LengthRouteConstraint>(constraint);
            Assert.Equal(5, ((LengthRouteConstraint)constraint).MinLength);
            Assert.Equal(5, ((LengthRouteConstraint)constraint).MaxLength);
        }

        [Fact]
        public void ResolveConstraint_LengthRangeConstraint()
        {
            // Arrange & Act
            var constraint = _constraintResolver.ResolveConstraint("length(5, 10)");

            // Assert
            Assert.IsType<LengthRouteConstraint>(constraint);
            var lengthConstraint = (LengthRouteConstraint)constraint;
            Assert.Equal(5, lengthConstraint.MinLength);
            Assert.Equal(10, lengthConstraint.MaxLength);
        }

        [Fact]
        public void ResolveConstraint_LongRangeConstraint()
        {
            // Arrange & Act
            var constraint = _constraintResolver.ResolveConstraint("long");

            // Assert
            Assert.IsType<LongRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_MaxConstraint()
        {
            // Arrange & Act
            var constraint = _constraintResolver.ResolveConstraint("max(10)");

            // Assert
            Assert.IsType<MaxRouteConstraint>(constraint);
            Assert.Equal(10, ((MaxRouteConstraint)constraint).Max);
        }

        [Fact]
        public void ResolveConstraint_MaxLengthConstraint()
        {
            // Arrange & Act
            var constraint = _constraintResolver.ResolveConstraint("maxlength(10)");

            // Assert
            Assert.IsType<MaxLengthRouteConstraint>(constraint);
            Assert.Equal(10, ((MaxLengthRouteConstraint)constraint).MaxLength);
        }

        [Fact]
        public void ResolveConstraint_MinConstraint()
        {
            // Arrange & Act
            var constraint = _constraintResolver.ResolveConstraint("min(3)");

            // Assert
            Assert.IsType<MinRouteConstraint>(constraint);
            Assert.Equal(3, ((MinRouteConstraint)constraint).Min);
        }

        [Fact]
        public void ResolveConstraint_MinLengthConstraint()
        {
            // Arrange & Act
            var constraint = _constraintResolver.ResolveConstraint("minlength(3)");

            // Assert
            Assert.IsType<MinLengthRouteConstraint>(constraint);
            Assert.Equal(3, ((MinLengthRouteConstraint)constraint).MinLength);
        }

        [Fact]
        public void ResolveConstraint_RangeConstraint()
        {
            // Arrange & Act
            var constraint = _constraintResolver.ResolveConstraint("range(5, 10)");

            // Assert
            Assert.IsType<RangeRouteConstraint>(constraint);
            var rangeConstraint = (RangeRouteConstraint)constraint;
            Assert.Equal(5, rangeConstraint.Min);
            Assert.Equal(10, rangeConstraint.Max);
        }

        [Fact]
        public void ResolveConstraint_SupportsCustomConstraints()
        {
            // Arrange
            var routeOptions = new RouteOptions();
            routeOptions.ConstraintMap.Add("custom", typeof(CustomRouteConstraint));
            var resolver = GetInlineConstraintResolver(routeOptions);

            // Act
            var constraint = resolver.ResolveConstraint("custom(argument)");

            // Assert
            Assert.IsType<CustomRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_CustomConstraintThatDoesNotImplementIRouteConstraint_Throws()
        {
            // Arrange
            var routeOptions = new RouteOptions();
            routeOptions.ConstraintMap.Add("custom", typeof(string));
            var resolver = GetInlineConstraintResolver(routeOptions);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => resolver.ResolveConstraint("custom"));
            Assert.Equal("The constraint type 'System.String' which is mapped to constraint key 'custom'"+
                         " must implement the 'IRouteConstraint' interface.", 
                         ex.Message);
        }

        [Fact]
        public void ResolveConstraint_AmbiguousConstructors_Throws()
        {
            // Arrange
            var routeOptions = new RouteOptions();
            routeOptions.ConstraintMap.Add("custom", typeof(MultiConstructorRouteConstraint));
            var resolver = GetInlineConstraintResolver(routeOptions);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => resolver.ResolveConstraint("custom(5,6)"));
            Assert.Equal("The constructor to use for activating the constraint type 'MultiConstructorRouteConstraint' is ambiguous." +
                         " Multiple constructors were found with the following number of parameters: 2.",
                         ex.Message);
        }

        [Fact]
        public void ResolveConstraint_NoMatchingConstructor_Throws()
        {
            // Arrange
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _constraintResolver.ResolveConstraint("int(5,6)"));
            Assert.Equal("Could not find a constructor for constraint type 'IntRouteConstraint'" +
                         " with the following number of parameters: 2.",
                         ex.Message);
        }

        private IInlineConstraintResolver GetInlineConstraintResolver(RouteOptions routeOptions)
        {
            var optionsAccessor = new Mock<IOptionsAccessor<RouteOptions>>();
            optionsAccessor.SetupGet(o => o.Options).Returns(routeOptions);
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(o => o.GetService(It.Is<Type>(type => type == typeof(ITypeActivator))))
                           .Returns(new TypeActivator());
            return new DefaultInlineConstraintResolver(serviceProvider.Object, optionsAccessor.Object);
        }

        private class MultiConstructorRouteConstraint : IRouteConstraint
        {
            public MultiConstructorRouteConstraint(string pattern, int intArg)
            {
            }

            public MultiConstructorRouteConstraint(int intArg, string pattern)
            {
            }

            public bool Match(HttpContext httpContext,
                              IRouter route,
                              string routeKey,
                              IDictionary<string, object> values,
                              RouteDirection routeDirection)
            {
                return true;
            }
        }

        private class CustomRouteConstraint : IRouteConstraint
        {
            public CustomRouteConstraint(string pattern)
            {
                Pattern = pattern;
            }

            public string Pattern { get; private set; }
            public bool Match(HttpContext httpContext,
                              IRouter route,
                              string routeKey,
                              IDictionary<string, object> values,
                              RouteDirection routeDirection)
            {
                return true;
            }
        }
    }
}
#endif
