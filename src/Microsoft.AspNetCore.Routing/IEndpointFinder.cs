using System.Collections.Generic;
using Microsoft.AspNetCore.Routing.Matchers;

namespace Microsoft.AspNetCore.Routing
{
    public interface IEndpointFinder
    {
        IEnumerable<MatcherEndpoint> FindEndpoints(Address address);
    }
}
