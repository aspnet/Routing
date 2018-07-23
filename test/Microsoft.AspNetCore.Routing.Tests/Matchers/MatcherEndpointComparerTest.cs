﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Routing.Patterns;
using Xunit;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    public class MatcherEndpointComparerTest
    {
        [Fact]
        public void Compare_PrefersOrder_IfDifferent()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/", order: 1);
            var endpoint2 = CreateEndpoint("/api/foo", order: -1);

            var comparer = CreateComparer();

            // Act
            var result = comparer.Compare(endpoint1, endpoint2);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void Compare_PrefersPrecedence_IfOrderIsSame()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/api/foo", order: 1);
            var endpoint2 = CreateEndpoint("/", order: 1);

            var comparer = CreateComparer();

            // Act
            var result = comparer.Compare(endpoint1, endpoint2);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void Compare_PrefersPolicy_IfPrecedenceIsSame()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/", order: 1, new TestMetadata1());
            var endpoint2 = CreateEndpoint("/", order: 1);

            var comparer = CreateComparer(new TestMetadata1Policy());

            // Act
            var result = comparer.Compare(endpoint1, endpoint2);

            // Assert
            Assert.Equal(-1, result);
        }

        [Fact]
        public void Compare_PrefersSecondPolicy_IfFirstPolicyIsSame()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/", order: 1, new TestMetadata1());
            var endpoint2 = CreateEndpoint("/", order: 1, new TestMetadata1(), new TestMetadata2());

            var comparer = CreateComparer(new TestMetadata1Policy(), new TestMetadata2Policy());

            // Act
            var result = comparer.Compare(endpoint1, endpoint2);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void Compare_PrefersTemplate_IfOtherCriteriaIsSame()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/foo", order: 1, new TestMetadata1());
            var endpoint2 = CreateEndpoint("/bar", order: 1, new TestMetadata1());

            var comparer = CreateComparer(new TestMetadata1Policy(), new TestMetadata2Policy());

            // Act
            var result = comparer.Compare(endpoint1, endpoint2);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void Compare_ReturnsZero_WhenIdentical()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/foo", order: 1, new TestMetadata1());
            var endpoint2 = CreateEndpoint("/foo", order: 1, new TestMetadata1());

            var comparer = CreateComparer(new TestMetadata1Policy(), new TestMetadata2Policy());

            // Act
            var result = comparer.Compare(endpoint1, endpoint2);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void Equals_NotEqual_IfOrderDifferent()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/", order: 1);
            var endpoint2 = CreateEndpoint("/api/foo", order: -1);

            var comparer = CreateComparer();

            // Act
            var result = comparer.Equals(endpoint1, endpoint2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Equals_NotEqual_IfPrecedenceDifferent()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/api/foo", order: 1);
            var endpoint2 = CreateEndpoint("/", order: 1);

            var comparer = CreateComparer();

            // Act
            var result = comparer.Equals(endpoint1, endpoint2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Equals_NotEqual_IfFirstPolicyDifferent()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/", order: 1, new TestMetadata1());
            var endpoint2 = CreateEndpoint("/", order: 1);

            var comparer = CreateComparer(new TestMetadata1Policy());

            // Act
            var result = comparer.Equals(endpoint1, endpoint2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Equals_NotEqual_IfSecondPolicyDifferent()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/", order: 1, new TestMetadata1());
            var endpoint2 = CreateEndpoint("/", order: 1, new TestMetadata1(), new TestMetadata2());

            var comparer = CreateComparer(new TestMetadata1Policy(), new TestMetadata2Policy());

            // Act
            var result = comparer.Equals(endpoint1, endpoint2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Equals_Equals_WhenTemplateIsDifferent()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/foo", order: 1, new TestMetadata1());
            var endpoint2 = CreateEndpoint("/bar", order: 1, new TestMetadata1());

            var comparer = CreateComparer(new TestMetadata1Policy(), new TestMetadata2Policy());

            // Act
            var result = comparer.Equals(endpoint1, endpoint2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Sort_MoreSpecific_FirstInList()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/foo", order: -1);
            var endpoint2 = CreateEndpoint("/bar/{baz}", order: -1);
            var endpoint3 = CreateEndpoint("/bar", order: 0, new TestMetadata1());
            var endpoint4 = CreateEndpoint("/foo", order: 0, new TestMetadata2());
            var endpoint5 = CreateEndpoint("/foo", order: 0);
            var endpoint6 = CreateEndpoint("/a{baz}", order: 0, new TestMetadata1(), new TestMetadata2());
            var endpoint7 = CreateEndpoint("/bar{baz}", order: 0, new TestMetadata1(), new TestMetadata2());

            // Endpoints listed in reverse of the desired order.
            var list = new List<MatcherEndpoint>() { endpoint7, endpoint6, endpoint5, endpoint4, endpoint3, endpoint2, endpoint1, };

            var comparer = CreateComparer(new TestMetadata1Policy(), new TestMetadata2Policy());

            // Act
            list.Sort(comparer);

            // Assert
            Assert.Collection(
                list,
                e => Assert.Same(endpoint1, e),
                e => Assert.Same(endpoint2, e),
                e => Assert.Same(endpoint3, e),
                e => Assert.Same(endpoint4, e),
                e => Assert.Same(endpoint5, e),
                e => Assert.Same(endpoint6, e),
                e => Assert.Same(endpoint7, e));
        }

        private static MatcherEndpoint CreateEndpoint(string template, int order, params object[] metadata)
        {
            return new MatcherEndpoint(
                MatcherEndpoint.EmptyInvoker,
                RoutePatternFactory.Parse(template),
                new RouteValueDictionary(),
                order,
                new EndpointMetadataCollection(metadata),
                "test: " + template);
        }

        private static MatcherEndpointComparer CreateComparer(params IEndpointComparerPolicy[] policies)
        {
            return new MatcherEndpointComparer(policies);
        }

        private class TestMetadata1
        {
        }

        private class TestMetadata1Policy : IEndpointComparerPolicy
        {
            public IComparer<Endpoint> Comparer => EndpointMetadataComparer<TestMetadata1>.Default;
        }

        private class TestMetadata2
        {
        }

        private class TestMetadata2Policy : IEndpointComparerPolicy
        {
            public IComparer<Endpoint> Comparer => EndpointMetadataComparer<TestMetadata2>.Default;
        }
    }
}
