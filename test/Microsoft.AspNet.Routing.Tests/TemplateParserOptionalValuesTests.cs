// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if DNX451

using System;
using System.Linq;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Routing.Tests
{
    public class TemplateParserOptionalValuesTests
    {
        private static IInlineConstraintResolver _inlineConstraintResolver = GetInlineConstraintResolver();

        [Fact]
        public void MandatoryParametersAreMarkedAsNotOptional()
        {
            // Arrange & Act
            var routeBuilder = CreateRouteBuilder();

            // Act
            routeBuilder.MapRoute("mockName", "{controller}/{action}/{name}");

            // Assert
            var optionals = ((Template.TemplateRoute)routeBuilder.Routes[0]).Optionals;
            Assert.False(optionals["name"]);
        }

        [Fact]
        public void NonMandatoryParametersAreMarkedAsOptional()
        {
            // Arrange & Act
            var routeBuilder = CreateRouteBuilder();

            // Act
            routeBuilder.MapRoute("mockName", "{controller}/{action}/{name?}");

            // Assert
            var optionals = ((Template.TemplateRoute)routeBuilder.Routes[0]).Optionals;
            Assert.True(optionals["name"]);
        }

        [Fact]
        public void CorrectNumberOfMandatoryParametershereDetectedWhenPresent()
        {
            // Arrange & Act
            var routeBuilder = CreateRouteBuilder();

            // Act
            routeBuilder.MapRoute("mockName", "test/{controller}/{action}/{name?}");

            // Assert
            var optionals = ((Template.TemplateRoute)routeBuilder.Routes[0]).Optionals;
            Assert.Equal(3, optionals.Keys.Count());
        }

        [Fact]
        public void CorrectNumberOfMandatoryParametershereDetectedWhenNotPresent()
        {
            // Arrange & Act
            var routeBuilder = CreateRouteBuilder();

            // Act
            routeBuilder.MapRoute("mockName", "test/hello");

            // Assert
            var optionals = ((Template.TemplateRoute)routeBuilder.Routes[0]).Optionals;
            Assert.Equal(0, optionals.Keys.Count());
        }

        private static IRouteBuilder CreateRouteBuilder()
        {
            var routeBuilder = new RouteBuilder();

            routeBuilder.DefaultHandler = new Mock<IRouter>().Object;

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(o => o.GetService(typeof(IInlineConstraintResolver)))
                               .Returns(_inlineConstraintResolver);
            routeBuilder.ServiceProvider = serviceProviderMock.Object;

            return routeBuilder;
        }

        private static IInlineConstraintResolver GetInlineConstraintResolver()
        {
            var services = new ServiceCollection().AddOptions();
            var serviceProvider = services.BuildServiceProvider();
            var accessor = serviceProvider.GetRequiredService<IOptions<RouteOptions>>();
            return new DefaultInlineConstraintResolver(accessor);
        }
    }
}

#endif
