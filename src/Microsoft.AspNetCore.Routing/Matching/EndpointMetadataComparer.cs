﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing.Matching
{
    /// <summary>
    /// A base class for <see cref="IComparer{Endpoint}"/> implementations that use 
    /// a specific type of metadata from <see cref="Endpoint.Metadata"/> for comparison.
    /// Useful for implementing <see cref="IEndpointComparerPolicy.Comparer"/>.
    /// </summary>
    /// <typeparam name="TMetadata">
    /// The type of metadata to compare. Typically this is a type of metadata related
    /// to the application concern being handled.
    /// </typeparam>
    public abstract class EndpointMetadataComparer<TMetadata> : IComparer<Endpoint> where TMetadata : class
    {
        public static readonly EndpointMetadataComparer<TMetadata> Default = new DefaultComparer<TMetadata>();

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, 
        /// or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// An implementation of this method must return a value less than zero if 
        /// x is less than y, zero if x is equal to y, or a value greater than zero if x is 
        /// greater than y.
        /// </returns>
        public int Compare(Endpoint x, Endpoint y)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            if (y == null)
            {
                throw new ArgumentNullException(nameof(y));
            }

            return CompareMetadata(GetMetadata(x), GetMetadata(y));
        }

        /// <summary>
        /// Gets the metadata of type <typeparamref name="TMetadata"/> from the provided endpoint.
        /// </summary>
        /// <param name="endpoint">The <see cref="Endpoint"/>.</param>
        /// <returns>The <typeparamref name="TMetadata"/> instance or <c>null</c>.</returns>
        protected virtual TMetadata GetMetadata(Endpoint endpoint)
        {
            return endpoint.Metadata.GetMetadata<TMetadata>();
        }

        /// <summary>
        /// Compares two <typeparamref name="TMetadata"/> instances.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// An implementation of this method must return a value less than zero if 
        /// x is less than y, zero if x is equal to y, or a value greater than zero if x is 
        /// greater than y.
        /// </returns>
        /// <remarks>
        /// The base-class implementation of this method will compare metadata based on whether
        /// or not they are <c>null</c>. The effect of this is that when endpoints are being
        /// compared, the endpoint that defines an instance of <typeparamref name="TMetadata"/>
        /// will be considered higher priority.
        /// </remarks>
        protected virtual int CompareMetadata(TMetadata x, TMetadata y)
        {
            // The default policy is that if x endpoint defines TMetadata, and
            // y endpoint does not, then x is *more specific* than y. We return
            // -1 for this case so that x will come first in the sort order.

            if (x == null && y != null)
            {
                // y is more specific
                return 1;
            }
            else if (x != null && y == null)
            {
                // x is more specific
                return -1;
            }

            // both endpoints have this metadata, or both do not have it, they have
            // the same specificity.
            return 0;
        }

        private class DefaultComparer<T> : EndpointMetadataComparer<T> where T : class
        {
        }
    }
}
