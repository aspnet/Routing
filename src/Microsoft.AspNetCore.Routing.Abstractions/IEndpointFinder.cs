using System.Collections.Generic;

namespace Microsoft.AspNetCore.Routing
{
    public interface IEndpointFinder
    {
        IEnumerable<Endpoint> FindEndpoints(Address address);
    }
}
