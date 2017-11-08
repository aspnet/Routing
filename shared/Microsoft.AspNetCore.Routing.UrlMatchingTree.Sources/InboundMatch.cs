// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
#if UrlMatching_InRouting
using Microsoft.AspNetCore.Routing.Template;
#else
using Microsoft.AspNetCore.Dispatcher;
#endif

#if UrlMatching_InRouting
namespace Microsoft.AspNetCore.Routing.Tree
#elif UrlMatching_InDispatcher
namespace Microsoft.AspNetCore.Dispatcher.Internal
#else
#error
#endif
{
#if UrlMatching_InRouting
    /// <summary>
    /// A candidate route to match incoming URLs in a <see cref="TreeRouter"/>.
    /// </summary>
    [DebuggerDisplay("{DebuggerToString(),nq}")]
    public
#else
    internal
#endif
    class InboundMatch
    {
        /// <summary>
        /// Gets or sets the <see cref="InboundRouteEntry"/>.
        /// </summary>
        public InboundRouteEntry Entry { get; set; }

#if UrlMatching_InRouting
        /// <summary>
        /// Gets or sets the <see cref="TemplateMatcher"/>.
        /// </summary>
        public TemplateMatcher TemplateMatcher { get; set; }

        private string DebuggerToString()
        {
            return TemplateMatcher?.Template?.TemplateText;
        }
#elif UrlMatching_InDispatcher

        /// <summary>
        /// Gets or sets the <see cref="RoutePatternMatcher"/>.
        /// </summary>
        public RoutePatternMatcher RoutePatternMatcher { get; set; }

        private string DebuggerToString()
        {
            return RoutePatternMatcher?.RoutePattern?.RawText;
        }
#else
#error
#endif
    }
}
