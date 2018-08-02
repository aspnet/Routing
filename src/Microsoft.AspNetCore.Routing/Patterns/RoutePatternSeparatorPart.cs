﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Microsoft.AspNetCore.Routing.Patterns
{
    /// <summary>
    /// Represents an optional separator part of a route pattern. Instances of <see cref="RoutePatternSeparatorPart"/>
    /// are immutable.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An optional separator is a literal text delimiter that appears between
    /// two parameter parts in the last segment of a route pattern. The only separator
    /// that is recognized is <c>.</c>.
    /// </para>
    /// <para>
    /// <example>
    /// In the route pattern <c>/{controller}/{action}/{id?}.{extension?}</c>
    /// the <c>.</c> character is an optional separator.
    /// </example>
    /// </para>
    /// <para>
    /// An optional separator character does not need to present in the URL path
    /// of a request for the route pattern to match.
    /// </para>
    /// </remarks>
    [DebuggerDisplay("{DebuggerToString()}")]
    public sealed class RoutePatternSeparatorPart : RoutePatternPart
    {
        internal RoutePatternSeparatorPart(string content)
            : base(RoutePatternPartKind.Separator)
        {
            Debug.Assert(!string.IsNullOrEmpty(content));

            Content = content;
        }

        /// <summary>
        /// Gets the text content of the part.
        /// </summary>
        public string Content { get; }

        internal override string DebuggerToString()
        {
            return Content;
        }
    }
}
