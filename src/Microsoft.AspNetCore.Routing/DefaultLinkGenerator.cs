using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace Microsoft.AspNetCore.Routing
{
    public class DefaultLinkGenerator : ILinkGenerator
    {
        private readonly IEndpointFinder _endpointFinder;
        private readonly ObjectPool<UriBuildingContext> _uriBuildingContextPool;
        private readonly ILogger<DefaultLinkGenerator> _logger;

        public DefaultLinkGenerator(
            IEndpointFinder endpointFinder,
            ObjectPool<UriBuildingContext> uriBuildingContextPool,
            ILogger<DefaultLinkGenerator> logger)
        {
            _endpointFinder = endpointFinder;
            _uriBuildingContextPool = uriBuildingContextPool;
            _logger = logger;
        }

        public string GetLink(Address address, IDictionary<string, object> values)
        {
            if (TryGetLink(address, values, out var link))
            {
                return link;
            }

            throw new InvalidOperationException("Could not find a matching endpoint to generate a link");
        }

        public bool TryGetLink(Address address, IDictionary<string, object> suppliedValues, out string link)
        {
            var endpoint = _endpointFinder.FindEndpoint(address);
            if (endpoint == null)
            {
                _logger.LogDebug($"Could not find an endpoint having an address with name '{address.Name}' and " +
                    $"MethodInfo '{address.MethodInfo.DeclaringType.FullName}.{address.MethodInfo.Name}'.");
            }

            link = GetLink(endpoint.RouteTemplate, endpoint.Values, suppliedValues);
            return link != null;
        }

        private string GetLink(
            RouteTemplate template,
            IReadOnlyDictionary<string, object> defaultValues,
            IDictionary<string, object> suppliedValues)
        {
            var defaults = new RouteValueDictionary(defaultValues);
            var templateBinder = new TemplateBinder(UrlEncoder.Default, _uriBuildingContextPool, template, defaults);
            return templateBinder.BindValues(new RouteValueDictionary(suppliedValues));
        }
    }
}
