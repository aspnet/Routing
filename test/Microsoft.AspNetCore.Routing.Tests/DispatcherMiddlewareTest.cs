// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Matchers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Routing
{
    public class DispatcherMiddlewareTest
    {
        [Fact]
        public async void Invoke_LogsCorrectValues_WhenNotHandled()
        {
            // Arrange
            var expectedMessage = "Request did not match any endpoints.";

            var sink = new TestSink(
                TestSink.EnableWithTypeName<DispatcherMiddleware>,
                TestSink.EnableWithTypeName<DispatcherMiddleware>);
            var loggerFactory = new TestLoggerFactory(sink, enabled: true);

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = new ServiceProvider();

            RequestDelegate next = (c) => Task.FromResult<object>(null);

            var logger = new Logger<DispatcherMiddleware>(loggerFactory);
            var options = Options.Create(new DispatcherOptions());
            var matcherFactory = new TestMatcherFactory(false);
            var middleware = new DispatcherMiddleware(matcherFactory, options, logger, next);

            // Act
            await middleware.Invoke(httpContext);

            // Assert
            Assert.Empty(sink.Scopes);
            var write = Assert.Single(sink.Writes);
            Assert.Equal(expectedMessage, write.State?.ToString());
        }

        [Fact]
        public async void Invoke_LogsCorrectValues_WhenHandled()
        {
            // Arrange
            var expectedMessage = "Request matched endpoint 'Test endpoint'.";

            var sink = new TestSink(
                TestSink.EnableWithTypeName<DispatcherMiddleware>,
                TestSink.EnableWithTypeName<DispatcherMiddleware>);
            var loggerFactory = new TestLoggerFactory(sink, enabled: true);

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = new ServiceProvider();

            RequestDelegate next = (c) => Task.FromResult<object>(null);

            var logger = new Logger<DispatcherMiddleware>(loggerFactory);
            var options = Options.Create(new DispatcherOptions());
            var matcherFactory = new TestMatcherFactory(true);
            var middleware = new DispatcherMiddleware(matcherFactory, options, logger, next);

            // Act
            await middleware.Invoke(httpContext);

            // Assert
            Assert.Empty(sink.Scopes);
            var write = Assert.Single(sink.Writes);
            Assert.Equal(expectedMessage, write.State?.ToString());
        }

        private class TestMatcherFactory : MatcherFactory
        {
            private readonly bool _isHandled;

            public TestMatcherFactory(bool isHandled)
            {
                _isHandled = isHandled;
            }

            public override Matcher CreateMatcher(EndpointDataSource dataSource)
            {
                return new TestMaster(_isHandled);
            }
        }

        private class TestMaster : Matcher
        {
            private readonly bool _isHandled;

            public TestMaster(bool isHandled)
            {
                _isHandled = isHandled;
            }

            public override Task MatchAsync(HttpContext httpContext, IEndpointFeature feature)
            {
                if (_isHandled)
                {
                    feature.Endpoint = new TestEndpoint(EndpointMetadataCollection.Empty, "Test endpoint");
                }

                return Task.CompletedTask;
            }
        }

        private class TestEndpoint : Endpoint
        {
            public TestEndpoint(EndpointMetadataCollection metadata, string displayName) : base(metadata, displayName)
            {
            }
        }

        private class ServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                throw new NotImplementedException();
            }
        }
    }
}
