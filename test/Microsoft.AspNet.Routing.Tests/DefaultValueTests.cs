﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET45

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing.Constraints;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.OptionsModel;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Routing.Tests
{
    public class DefaultValueTests
    {
        private static IInlineConstraintResolver _inlineConstraintResolver = GetInlineConstraintResolver();

        [Fact]
        public void InlineDefaultValueSpecified_InlineValueIsUsed()
        {
            // Arrange & Act
            var routeBuilder = CreateRouteBuilder();

            // Act
            routeBuilder.MapRoute("mockName",
                "{controller}/{action}/{id:int=12}",
                defaults: null,
                constraints: null);

            // Assert
            var defaults = ((Template.TemplateRoute)routeBuilder.Routes[0]).Defaults;
            Assert.Equal("12", defaults["id"]);
        }

        [Fact]
        public void ExplicitDefaultValueSpecified_WithInlineDefaultValue_Throws()
        {
            // Arrange
            var routeBuilder = CreateRouteBuilder();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(
                                () => routeBuilder.MapRoute("mockName",
                                                            "{controller}/{action}/{id:int=12}",
                                                            defaults: new { id = 13 },
                                                            constraints: null));

            var message = "The route parameter 'id' has both an inline default value and an explicit default" +
                          " value specified. A route parameter cannot contain an inline default value when" +
                          " a default value is specified explicitly. Consider removing one of them.";
            Assert.Equal(message, ex.Message);
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
            var services = new ServiceCollection { OptionsServices.GetDefaultServices() };
            var serviceProvider = services.BuildServiceProvider();
            var accessor = serviceProvider.GetService<IOptionsAccessor<RouteOptions>>();
            return new DefaultInlineConstraintResolver(serviceProvider, accessor);
        }
    }
}

#endif