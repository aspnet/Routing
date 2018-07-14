﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Builder
{
    public class DispatcherApplicationBuilderExtensionsTest
    {
        [Fact]
        public void UseDispatcher_ServicesNotRegistered_Throws()
        {
            // Arrange
            var app = new ApplicationBuilder(Mock.Of<IServiceProvider>());

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => app.UseDispatcher());

            // Assert
            Assert.Equal(
                "Unable to find the required services. " +
                "Please add all the required services by calling 'IServiceCollection.AddDispatcher' " +
                "inside the call to 'ConfigureServices(...)' in the application startup code.",
                ex.Message);
        }

        [Fact]
        public void UseEndpoint_ServicesNotRegistered_Throws()
        {
            // Arrange
            var app = new ApplicationBuilder(Mock.Of<IServiceProvider>());

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => app.UseEndpoint());

            // Assert
            Assert.Equal(
                "Unable to find the required services. " +
                "Please add all the required services by calling 'IServiceCollection.AddDispatcher' " +
                "inside the call to 'ConfigureServices(...)' in the application startup code.",
                ex.Message);
        }

        [Fact]
        public async Task UseDispatcher_ServicesRegistered_SetsFeature()
        {
            // Arrange
            var services = CreateServices();

            var app = new ApplicationBuilder(services);

            app.UseDispatcher();

            var appFunc = app.Build();
            var httpContext = new DefaultHttpContext();

            // Act
            await appFunc(httpContext);

            // Assert
            Assert.NotNull(httpContext.Features.Get<IEndpointFeature>());
        }

        [Fact]
        public void UseEndpoint_ServicesRegisteredAndNoDispatcherRegistered_Throws()
        {
            // Arrange
            var services = CreateServices();

            var app = new ApplicationBuilder(services);

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => app.UseEndpoint());

            // Assert
            Assert.Equal(
                "DispatcherMiddleware must be added to the request execution pipeline before EndpointMiddleware. " +
                "Please add DispatcherMiddleware by calling 'IApplicationBuilder.UseDispatcher' " +
                "inside the call to 'Configure(...)' in the application startup code.",
                ex.Message);
        }

        [Fact]
        public async Task UseEndpoint_ServicesRegisteredAndDispatcherRegistered_SetsFeature()
        {
            // Arrange
            var services = CreateServices();

            var app = new ApplicationBuilder(services);

            app.UseDispatcher();
            app.UseEndpoint();

            var appFunc = app.Build();
            var httpContext = new DefaultHttpContext();

            // Act
            await appFunc(httpContext);

            // Assert
            Assert.NotNull(httpContext.Features.Get<IEndpointFeature>());
        }

        private IServiceProvider CreateServices()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddOptions();
            services.AddDispatcher();

            return services.BuildServiceProvider();
        }
    }
}
