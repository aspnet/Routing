using System.Collections.Generic;

namespace Microsoft.AspNetCore.Routing
{
    public class LinkGeneratorContext
    {
        public Address Address { get; set; }

        public IDictionary<string, object> AmbientValues { get; set; }

        // values explicitly set by users
        public IDictionary<string, object> SuppliedValues { get; set; }
    }
}
