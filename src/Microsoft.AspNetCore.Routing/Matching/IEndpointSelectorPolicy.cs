﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Routing.Matching
{
    /// <summary>
    /// A <see cref="MatcherPolicy"/> interface that can implemented to filter endpoints
    /// in a <see cref="CandidateSet"/>. Implementations of <see cref="IEndpointSelectorPolicy"/> must
    /// inherit from <see cref="MatcherPolicy"/> and should be registered in
    /// the dependency injection container as singleton services of type <see cref="MatcherPolicy"/>.
    /// </summary>
    public interface IEndpointSelectorPolicy
    {
        /// <summary>
        /// Returns a value that indicates whether the <see cref="IEndpointSelectorPolicy"/> applies
        /// to any endpoint in <paramref name="endpoints"/>.
        /// </summary>
        /// <param name="endpoints">The set of candidate <see cref="Endpoint"/> values.</param>
        /// <returns>
        /// <c>true</c> if the policy applies to any endpoint in <paramref name="endpoints"/>, otherwise <c>false</c>.
        /// </returns>
        bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints);

        /// <summary>
        /// Applies the policy to the <see cref="CandidateSet"/>.
        /// </summary>
        /// <param name="httpContext">
        /// The <see cref="HttpContext"/> associated with the current request.
        /// </param>
        /// <param name="context">
        /// The <see cref="EndpointSelectorContext"/> associated with the current request.
        /// </param>
        /// <param name="candidates">The <see cref="CandidateSet"/>.</param>
        /// <remarks>
        /// <para>
        /// Implementations of <see cref="IEndpointSelectorPolicy"/> should implement this method
        /// and filter the set of candidates in the <paramref name="candidates"/> by setting
        /// <see cref="CandidateSet.SetValidity(int, bool)"/> to <c>false</c> where desired.
        /// </para>
        /// <para>
        /// To signal an error condition, set <see cref="EndpointSelectorContext.Endpoint"/> to an
        /// <see cref="Endpoint"/> value that will produce the desired error when executed.
        /// </para>
        /// </remarks>
        Task ApplyAsync(HttpContext httpContext, EndpointSelectorContext context, CandidateSet candidates);
    }
}
