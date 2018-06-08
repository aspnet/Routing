using Microsoft.AspNetCore.Routing.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Routing
{
    public class DefaultEndpointFinder : IEndpointFinder
    {
        private readonly IEnumerable<EndpointDataSource> _endpointDatasources;

        public DefaultEndpointFinder(IEnumerable<EndpointDataSource> endpointDataSources)
        {
            _endpointDatasources = endpointDataSources;
        }

        public MatcherEndpoint FindEndpoint(Address address)
        {
            if (string.IsNullOrEmpty(address.Name) && address.MethodInfo == null)
            {
                return null;
            }

            var compositeEndpointDatasource = new CompositeEndpointDataSource(_endpointDatasources);
            var matcherEndpoints = compositeEndpointDatasource.Endpoints
                .OfType<MatcherEndpoint>()
                .Where(mep => mep.Address != null);

            foreach (var endpoint in matcherEndpoints)
            {
                if (!string.IsNullOrEmpty(address.Name) &&
                    !string.IsNullOrEmpty(endpoint.Address.Name) &&
                    string.Equals(address.Name, endpoint.Address.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return endpoint;
                }

                if (address.MethodInfo != null &&
                    address.MethodInfo.Equals(endpoint.Address.MethodInfo))
                {
                    return endpoint;
                }
            }

            return null;
        }
    }
}
