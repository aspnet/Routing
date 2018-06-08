// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DispatcherSample.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Microsoft.AspNetCore.Routing.FunctionalTests
{
    public class DispatcherSampleLinkGeneratorTest : IDisposable
    {
        private readonly HttpClient _client;
        private readonly TestServer _testServer;

        public DispatcherSampleLinkGeneratorTest()
        {
            var webHostBuilder = DispatcherSample.Web.Program.GetWebHostBuilder();
            webHostBuilder.UseStartup<StartupUsingLinkGenerator>();
            _testServer = new TestServer(webHostBuilder);
            _client = _testServer.CreateClient();
            _client.BaseAddress = new Uri("http://localhost");
        }

        [Fact]
        public async Task GeneratesLink_UsingAddress_MethodInfo()
        {
            // Arrange 1
            var expectedOutput = $"<html><body><a href=\"/MainHub/Contact\">Contact</a></body></html>";

            // Act 1
            var response = await _client.GetAsync("/mainhub/index");

            // Assert 1
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(content, expectedOutput);

            // Arrange 2
            expectedOutput = "Hello, World!";

            // Act 2
            response = await _client.GetAsync("/mainhub/contact");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            content = await response.Content.ReadAsStringAsync();
            Assert.Equal(content, expectedOutput);
        }

        public void Dispose()
        {
            _testServer.Dispose();
            _client.Dispose();
        }
    }
}
