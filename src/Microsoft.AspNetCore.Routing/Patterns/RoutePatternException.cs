﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.Serialization;

namespace Microsoft.AspNetCore.Routing.Patterns
{
    /// <summary>
    /// An exception that is thrown for error constructing a <see cref="RoutePattern"/>.
    /// </summary>
    [Serializable]
    public sealed class RoutePatternException : Exception
    {
        private RoutePatternException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Pattern = (string)info.GetValue(nameof(Pattern), typeof(string));
        }

        /// <summary>
        /// Creates a new instance of <see cref="RoutePatternException"/>.
        /// </summary>
        /// <param name="pattern">The route pattern as raw text.</param>
        /// <param name="message">The exception message.</param>
        public RoutePatternException(string pattern, string message)
            : base(message)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            Pattern = pattern;
        }

        /// <summary>
        /// Gets the route pattern associated with this exception.
        /// </summary>
        public string Pattern { get; }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (<see cref="StreamingContext" />) for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Pattern), Pattern);
            base.GetObjectData(info, context);
        }
    }
}
