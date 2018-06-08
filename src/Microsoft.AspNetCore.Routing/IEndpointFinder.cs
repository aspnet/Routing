using Microsoft.AspNetCore.Routing.Matchers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Routing
{
    public interface IEndpointFinder
    {
        MatcherEndpoint FindEndpoint(Address address);
    }
}
