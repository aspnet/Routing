using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing.Matchers;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Routing
{
    public class DefaultEndpointFinder : IEndpointFinder
    {
        private readonly CompositeEndpointDataSource _endpointDatasource;
        private readonly ILogger<DefaultEndpointFinder> _logger;

        public DefaultEndpointFinder(
            CompositeEndpointDataSource endpointDataSource,
            ILogger<DefaultEndpointFinder> logger)
        {
            _endpointDatasource = endpointDataSource;
            _logger = logger;
        }

        public IEnumerable<MatcherEndpoint> FindEndpoints(Address lookupAddress)
        {
            if (lookupAddress == null ||
                (string.IsNullOrEmpty(lookupAddress.Name) &&
                lookupAddress.MethodInfo == null))
            {
                return Enumerable.Empty<MatcherEndpoint>();
            }

            var matcherEndpoints = _endpointDatasource.Endpoints
                .OfType<MatcherEndpoint>()
                .Where(mep => mep.Address != null);

            var result = new List<MatcherEndpoint>();
            foreach (var endpoint in matcherEndpoints)
            {
                if (!string.IsNullOrEmpty(lookupAddress.Name) &&
                    !string.IsNullOrEmpty(endpoint.Address.Name) &&
                    string.Equals(lookupAddress.Name, endpoint.Address.Name, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(endpoint);
                }

                if (lookupAddress.MethodInfo != null &&
                    lookupAddress.MethodInfo.Equals(endpoint.Address.MethodInfo))
                {
                    result.Add(endpoint);
                    break;
                }
            }

            if (result.Count == 0)
            {
                _logger.LogDebug(
                    $"Could not find endpoint(s) having an address with name '{lookupAddress.Name}' or " +
                    $"MethodInfo '{lookupAddress.MethodInfo?.DeclaringType.FullName}.{lookupAddress.MethodInfo?.Name}'.");
            }

            return result;
        }
    }
}
