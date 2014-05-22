// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET45

using System;
using Xunit;

namespace Microsoft.AspNet.Routing.Tests
{
    public class DefaultInlineConstraintResolverTest
    {
        [Fact]
        public void ResolveConstraint_IntConstraint_ResolvesCorrectly()
        {
            // Arrange & Act
            var constraint = new DefaultInlineConstraintResolver().ResolveConstraint("int");

            // Assert
            Assert.IsType<IntRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_IntConstraintWithArgument_Throws()
        {
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(
                () => new DefaultInlineConstraintResolver().ResolveConstraint("int(5)"));
            Assert.Equal("Could not find a constructor for constraint type 'IntRouteConstraint'"+
                         " with the following number of parameters: 1.",
                         ex.Message);
        }

        [Fact]
        public void ResolveConstraint_SupportsCustomConstraints()
        {
            // Arrange
            var resolver = new DefaultInlineConstraintResolver();
            resolver.ConstraintMap.Add("custom", typeof(IntRouteConstraint));

            // Act
            var constraint = resolver.ResolveConstraint("custom");

            // Assert
            Assert.IsType<IntRouteConstraint>(constraint);
        }

        [Fact]
        public void ResolveConstraint_CustomConstraintThatDoesNotImplementouteConstraintInterfact_Throws()
        {
            // Arrange
            var resolver = new DefaultInlineConstraintResolver();
            resolver.ConstraintMap.Add("custom", typeof(string));

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => resolver.ResolveConstraint("custom"));
            Assert.Equal("The constraint type 'System.String' which is mapped to constraint key 'custom'"+
                         " must implement the IRouteConstraint interface.", 
                         ex.Message);
        }
    }
}

#endif
