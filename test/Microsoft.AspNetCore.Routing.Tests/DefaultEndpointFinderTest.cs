using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing.Matchers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Routing
{
    public class DefaultEndpointFinderTest
    {
        [Fact]
        public void FindEndpoints_IgnoresCase_ForRouteNameLookup()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/home", new Address("home"));
            var endpoint2 = CreateEndpoint("/admin", new Address("admin"));
            var endpointFinder = CreateDefaultEndpointFinder(endpoint1, endpoint2);

            // Act
            var result = endpointFinder.FindEndpoints(new Address("Admin"));

            // Assert
            var endpoint = Assert.Single(result);
            Assert.Same(endpoint2, endpoint);
        }

        [Fact]
        public void FindEndpoints_FindsEndpoint_WithMethodInfo()
        {
            // Arrange
            var lookupMethodInfo = typeof(AdminController).GetMethod("Contact");
            var endpoint1 = CreateEndpoint("/home/index", new Address(typeof(HomeController).GetMethod("Index")));
            var endpoint2 = CreateEndpoint("/home/contact", new Address(typeof(HomeController).GetMethod("Contact")));
            var endpoint3 = CreateEndpoint("/admin/contact", new Address(lookupMethodInfo));
            var endpointFinder = CreateDefaultEndpointFinder(endpoint1, endpoint2, endpoint3);

            // Act
            var result = endpointFinder.FindEndpoints(new Address(lookupMethodInfo));

            // Assert
            var endpoint = Assert.Single(result);
            Assert.Same(endpoint3, endpoint);
        }

        [Fact]
        public void FindEndpoints_FindsFirstEndpoint_WithMatchingMethodInfo()
        {
            // Arrange
            var lookupMethodInfo = typeof(AdminController).GetMethod("Contact");
            var endpoint1 = CreateEndpoint("/home/index", new Address(typeof(HomeController).GetMethod("Index")));
            var endpoint2 = CreateEndpoint("/admin/contact", new Address(lookupMethodInfo));
            var endpoint3 = CreateEndpoint("/admin/foo", new Address(lookupMethodInfo));
            var endpointFinder = CreateDefaultEndpointFinder(endpoint1, endpoint2, endpoint3);

            // Act
            var result = endpointFinder.FindEndpoints(new Address(lookupMethodInfo));

            // Assert
            var endpoint = Assert.Single(result);
            Assert.Same(endpoint2, endpoint);
        }

        [Fact]
        public void FindEndpoints_FindsMultipleEndpoints_WithMatchingName()
        {
            // Arrange
            var name = "common-tag-for-all-my-section's-routes";
            var endpoint1 = CreateEndpoint("/home", new Address(name));
            var endpoint2 = CreateEndpoint("/admin", new Address("admin"));
            var endpoint3 = CreateEndpoint("/customers", new Address(name));
            var endpoint4 = CreateEndpoint("/products", new Address("products"));
            var endpointFinder = CreateDefaultEndpointFinder(endpoint1, endpoint2, endpoint3, endpoint4);

            // Act
            var result = endpointFinder.FindEndpoints(new Address(name));

            // Assert
            Assert.Collection(
                result,
                (ep) => Assert.Same(endpoint1, ep),
                (ep) => Assert.Same(endpoint3, ep));
        }

        [Fact]
        public void FindEndpoints_ReturnsEmpty_WhenLookupAddress_IsNull()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/home", new Address("home"));
            var endpoint2 = CreateEndpoint("/admin", new Address("admin"));
            var endpointFinder = CreateDefaultEndpointFinder(endpoint1, endpoint2);

            // Act
            var result = endpointFinder.FindEndpoints(lookupAddress: null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FindEndpoints_ReturnsEmpty_WhenNoInformationGiven_OnLookupAddress()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/home", new Address("home"));
            var endpoint2 = CreateEndpoint("/admin", new Address("admin"));
            var endpointFinder = CreateDefaultEndpointFinder(endpoint1, endpoint2);

            // Act
            var result = endpointFinder.FindEndpoints(new Address(name: null, methodInfo: null));

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FindEndpoints_ReturnsEmpty_WhenNoEndpointFound_WithLookupAddress_Name()
        {
            // Arrange
            var endpoint1 = CreateEndpoint("/home", new Address("home"));
            var endpoint2 = CreateEndpoint("/admin", new Address("admin"));
            var endpointFinder = CreateDefaultEndpointFinder(endpoint1, endpoint2);

            // Act
            var result = endpointFinder.FindEndpoints(new Address("DoesNotExists"));

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FindEndpoints_ReturnsEmpty_WhenNoEndpointFound_WithLookupAddress_Method()
        {
            // Arrange
            var lookupMethodInfo = typeof(AdminController).GetMethod("Contact");
            var endpoint1 = CreateEndpoint("/home/index", new Address(typeof(HomeController).GetMethod("Index")));
            var endpoint2 = CreateEndpoint("/home/contact", new Address(typeof(HomeController).GetMethod("Contact")));
            var endpoint3 = CreateEndpoint("/admin/index", new Address(typeof(AdminController).GetMethod("Index")));
            var endpointFinder = CreateDefaultEndpointFinder(endpoint1, endpoint2, endpoint3);

            // Act
            var result = endpointFinder.FindEndpoints(new Address(lookupMethodInfo));

            // Assert
            Assert.Empty(result);
        }

        private MatcherEndpoint CreateEndpoint(string template, Address address)
        {
            return new MatcherEndpoint(
                next => (httpContext) => Task.CompletedTask,
                template,
                null,
                0,
                EndpointMetadataCollection.Empty,
                null,
                address);
        }

        private DefaultEndpointFinder CreateDefaultEndpointFinder(params MatcherEndpoint[] endpoints)
        {
            return new DefaultEndpointFinder(
                new CompositeEndpointDataSource(new[] { new DefaultEndpointDataSource(endpoints) }),
                Mock.Of<ILogger<DefaultEndpointFinder>>());
        }

        private class HomeController
        {
            public void Index() { }
            public void Contact() { }
        }

        private class AdminController
        {
            public void Index() { }
            public void Contact() { }
        }
    }
}
