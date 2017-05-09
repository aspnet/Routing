using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Routing.Constraints
{
    /// <summary>
    /// Represents a partial match regex constraint which can be used as an inlineConstraint.
    /// </summary>
    public class PartialRegexInlineRouteConstraint : RegexRouteConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegexInlineRouteConstraint" /> class.
        /// </summary>
        /// <param name="regexPattern">The regular expression pattern to match.</param>
        public PartialRegexInlineRouteConstraint(string regexPattern)
            : base(regexPattern)
        {
        }
    }
}
