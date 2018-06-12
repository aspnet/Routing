using System;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<Endpoint> FindEndpoints(Address lookupAddress)
        {
            var allEndpoints = _endpointDatasource.Endpoints;

            if (lookupAddress == null ||
                (string.IsNullOrEmpty(lookupAddress.Name) &&
                lookupAddress.MethodInfo == null))
            {
                return allEndpoints;
            }

            var endpointsWithAddress = allEndpoints.Where(ep => ep.Address != null);
            if (!endpointsWithAddress.Any())
            {
                return allEndpoints;
            }

            var result = new List<Endpoint>();
            foreach (var endpoint in endpointsWithAddress)
            {
                if (lookupAddress.MethodInfo != null &&
                    lookupAddress.MethodInfo.Equals(endpoint.Address.MethodInfo))
                {
                    result.Add(endpoint);
                    break;
                }

                if (!string.IsNullOrEmpty(lookupAddress.Name) &&
                    !string.IsNullOrEmpty(endpoint.Address.Name) &&
                    string.Equals(lookupAddress.Name, endpoint.Address.Name, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(endpoint);
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
