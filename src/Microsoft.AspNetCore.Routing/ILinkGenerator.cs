using Microsoft.AspNetCore.Routing.Matchers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Routing
{
    public interface ILinkGenerator
    {
        bool TryGetLink(Address address, IDictionary<string, object> values, out string link);

        string GetLink(Address address, IDictionary<string, object> values);
    }
}
