// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Dispatcher.FunctionalTest
{
    public class ApiAppTest : IClassFixture<ApiAppFixture>
    {
        public ApiAppTest(ApiAppFixture app)
        {
            Client = app.Client;
        }

        public HttpClient Client { get; }

        [Fact]
        public async Task ApiApp_CanRouteTo_LiteralEndpoint()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/products");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Hello, Products_Get", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task ApiApp_RoutesTo_EndpointWithMatchingHttpMethod()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/products");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Hello, Products_Post", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task ApiApp_RoutesTo_EndpointWithMatchingHttpMethod_DoesNotMatchExpectedRoute()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Put, "/api/services");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ApiApp_NoEndpointWithMatchingHttpMethod()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Delete, "/api/products");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
