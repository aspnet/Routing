using Microsoft.AspNetCore.Routing.TestObjects;
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
            var endpoint1 = CreateEndpoint(new Address("home"));
            var endpoint2 = CreateEndpoint(new Address("admin"));
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
            var endpoint1 = CreateEndpoint(new Address(typeof(HomeController).GetMethod("Index")));
            var endpoint2 = CreateEndpoint(new Address(typeof(HomeController).GetMethod("Contact")));
            var endpoint3 = CreateEndpoint(new Address(lookupMethodInfo));
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
            var endpoint1 = CreateEndpoint(new Address(typeof(HomeController).GetMethod("Index")));
            var endpoint2 = CreateEndpoint(new Address(lookupMethodInfo));
            var endpoint3 = CreateEndpoint(new Address(lookupMethodInfo));
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
            var endpoint1 = CreateEndpoint(new Address(name));
            var endpoint2 = CreateEndpoint(new Address("admin"));
            var endpoint3 = CreateEndpoint(new Address(name));
            var endpoint4 = CreateEndpoint(new Address("products"));
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
        public void FindEndpoints_ReturnsAllEndpoints_WhenNoEndpointsHaveAddress()
        {
            // Arrange
            var endpoint1 = CreateEndpoint(address: null);
            var endpoint2 = CreateEndpoint(address: null);
            var endpointFinder = CreateDefaultEndpointFinder(endpoint1, endpoint2);

            // Act
            var result = endpointFinder.FindEndpoints(new Address("Admin"));

            // Assert
            Assert.Collection(
                result,
                (ep) => Assert.Same(endpoint1, ep),
                (ep) => Assert.Same(endpoint2, ep));
        }

        [Fact]
        public void FindEndpoints_ReturnsAllEndpoints_WhenLookupAddress_IsNull()
        {
            // Arrange
            var endpoint1 = CreateEndpoint(new Address("home"));
            var endpoint2 = CreateEndpoint(new Address("admin"));
            var endpointFinder = CreateDefaultEndpointFinder(endpoint1, endpoint2);

            // Act
            var result = endpointFinder.FindEndpoints(lookupAddress: null);

            // Assert
            Assert.Collection(
                result,
                (ep) => Assert.Same(endpoint1, ep),
                (ep) => Assert.Same(endpoint2, ep));
        }

        [Fact]
        public void FindEndpoints_ReturnsAllEndpoints_WhenNoEndpointsHaveAddress_AndLookupAddress_IsNull()
        {
            // Arrange
            var endpoint1 = CreateEndpoint(address: null);
            var endpoint2 = CreateEndpoint(address: null);
            var endpointFinder = CreateDefaultEndpointFinder(endpoint1, endpoint2);

            // Act
            var result = endpointFinder.FindEndpoints(lookupAddress: null);

            // Assert
            Assert.Collection(
                result,
                (ep) => Assert.Same(endpoint1, ep),
                (ep) => Assert.Same(endpoint2, ep));
        }

        [Fact]
        public void FindEndpoints_ReturnsAllEndpoints_WhenNoInformationGiven_OnLookupAddress()
        {
            // Arrange
            var endpoint1 = CreateEndpoint(new Address("home"));
            var endpoint2 = CreateEndpoint(new Address("admin"));
            var endpointFinder = CreateDefaultEndpointFinder(endpoint1, endpoint2);

            // Act
            var result = endpointFinder.FindEndpoints(new Address(name: null, methodInfo: null));

            // Assert
            Assert.Collection(
                result,
                (ep) => Assert.Same(endpoint1, ep),
                (ep) => Assert.Same(endpoint2, ep));
        }

        [Fact]
        public void FindEndpoints_ReturnsEmpty_WhenNoEndpointFound_WithLookupAddress_Name()
        {
            // Arrange
            var endpoint1 = CreateEndpoint(new Address("home"));
            var endpoint2 = CreateEndpoint(new Address("admin"));
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
            var endpoint1 = CreateEndpoint(new Address(typeof(HomeController).GetMethod("Index")));
            var endpoint2 = CreateEndpoint(new Address(typeof(HomeController).GetMethod("Contact")));
            var endpoint3 = CreateEndpoint(new Address(typeof(AdminController).GetMethod("Index")));
            var endpointFinder = CreateDefaultEndpointFinder(endpoint1, endpoint2, endpoint3);

            // Act
            var result = endpointFinder.FindEndpoints(new Address(lookupMethodInfo));

            // Assert
            Assert.Empty(result);
        }

        private Endpoint CreateEndpoint(Address address)
        {
            return new TestEndpoint(
                EndpointMetadataCollection.Empty,
                displayName: null,
                address: address);
        }

        private DefaultEndpointFinder CreateDefaultEndpointFinder(params Endpoint[] endpoints)
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
