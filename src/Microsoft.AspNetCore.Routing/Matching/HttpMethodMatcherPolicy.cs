﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Routing.Matching
{
    /// <summary>
    /// An <see cref="MatcherPolicy"/> that implements filtering and selection by
    /// the HTTP method of a request.
    /// </summary>
    public sealed class HttpMethodMatcherPolicy : MatcherPolicy, IEndpointComparerPolicy, INodeBuilderPolicy
    {
        // Used in tests
        internal static readonly string OriginHeader = "Origin";
        internal static readonly string AccessControlRequestMethod = "Access-Control-Request-Method";
        internal static readonly string PreflightHttpMethod = "OPTIONS";

        // Used in tests
        internal const string Http405EndpointDisplayName = "405 HTTP Method Not Supported";

        // Used in tests
        internal const string AnyMethod = "*";

        /// <summary>
        /// For framework use only.
        /// </summary>
        public IComparer<Endpoint> Comparer => new HttpMethodMetadataEndpointComparer();

        // The order value is chosen to be less than 0, so that it comes before naively
        // written policies.
        /// <summary>
        /// For framework use only.
        /// </summary>
        public override int Order => -1000;

        /// <summary>
        /// For framework use only.
        /// </summary>
        /// <param name="endpoints"></param>
        /// <returns></returns>
        public bool AppliesToNode(IReadOnlyList<Endpoint> endpoints)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            for (var i = 0; i < endpoints.Count; i++)
            {
                if (endpoints[i].Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Any() == true)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// For framework use only.
        /// </summary>
        /// <param name="endpoints"></param>
        /// <returns></returns>
        public IReadOnlyList<PolicyNodeEdge> GetEdges(IReadOnlyList<Endpoint> endpoints)
        {
            // The algorithm here is designed to be preserve the order of the endpoints
            // while also being relatively simple. Preserving order is important.

            // First, build a dictionary of all possible HTTP method/CORS combinations
            // that exist in this list of endpoints.
            //
            // For now we're just building up the set of keys. We don't add any endpoints
            // to lists now because we don't want ordering problems.
            var allHttpMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var edges = new Dictionary<EdgeKey, List<Endpoint>>();
            for (var i = 0; i < endpoints.Count; i++)
            {
                var endpoint = endpoints[i];
                var (httpMethods, acceptCorsPreFlight) = GetHttpMethods(endpoint);

                // If the action doesn't list HTTP methods then it supports all methods.
                // In this phase we use a sentinel value to represent the *other* HTTP method
                // a state that represents any HTTP method that doesn't have a match.
                if (httpMethods.Count == 0)
                {
                    httpMethods = new[] { AnyMethod, };
                }

                for (var j = 0; j < httpMethods.Count; j++)
                {
                    // An endpoint that allows CORS reqests will match both CORS and non-CORS
                    // so we model it as both.
                    var httpMethod = httpMethods[j];
                    var key = new EdgeKey(httpMethod, acceptCorsPreFlight);
                    if (!edges.ContainsKey(key))
                    {
                        edges.Add(key, new List<Endpoint>());
                    }

                    // An endpoint that allows CORS reqests will match both CORS and non-CORS
                    // so we model it as both.
                    if (acceptCorsPreFlight)
                    {
                        key = new EdgeKey(httpMethod, false);
                        if (!edges.ContainsKey(key))
                        {
                            edges.Add(key, new List<Endpoint>());
                        }
                    }

                    // Also if it's not the *any* method key, then track it.
                    if (!string.Equals(AnyMethod, httpMethod, StringComparison.OrdinalIgnoreCase))
                    {
                        allHttpMethods.Add(httpMethod);
                    }
                }
            }

            // Now in a second loop, add endpoints to these lists. We've enumerated all of
            // the states, so we want to see which states this endpoint matches.
            for (var i = 0; i < endpoints.Count; i++)
            {
                var endpoint = endpoints[i];
                var (httpMethods, acceptCorsPreFlight) = GetHttpMethods(endpoint);

                if (httpMethods.Count == 0)
                {
                    // OK this means that this endpoint matches *all* HTTP methods.
                    // So, loop and add it to all states.
                    foreach (var kvp in edges)
                    {
                        if (acceptCorsPreFlight || !kvp.Key.IsCorsPreflightRequest)
                        {
                            kvp.Value.Add(endpoint);
                        }
                    }
                }
                else
                {
                    // OK this endpoint matches specific methods.
                    for (var j = 0; j < httpMethods.Count; j++)
                    {
                        var httpMethod = httpMethods[j];
                        var key = new EdgeKey(httpMethod, acceptCorsPreFlight);

                        edges[key].Add(endpoint);

                        // An endpoint that allows CORS reqests will match both CORS and non-CORS
                        // so we model it as both.
                        if (acceptCorsPreFlight)
                        {
                            key = new EdgeKey(httpMethod, false);
                            edges[key].Add(endpoint);
                        }
                    }
                }
            }

            // Adds a very low priority endpoint that will reject the request with
            // a 405 if nothing else can handle this verb. This is only done if
            // no other actions exist that handle the 'all verbs'.
            //
            // The rationale for this is that we want to report a 405 if none of
            // the supported methods match, but we don't want to report a 405 in a
            // case where an application defines an endpoint that handles all verbs, but
            // a constraint rejects the request, or a complex segment fails to parse. We
            // consider a case like that a 'user input validation' failure  rather than
            // a semantic violation of HTTP.
            //
            // This will make 405 much more likely in API-focused applications, and somewhat
            // unlikely in a traditional MVC application. That's good.
            //
            // We don't bother returning a 405 when the CORS preflight method doesn't exist.
            // The developer calling the API will see it as a CORS error, which is fine because 
            // there isn't an endpoint to check for a CORS policy.
            if (!edges.TryGetValue(new EdgeKey(AnyMethod, false), out var matches))
            {
                // Methods sorted for testability.
                var endpoint = CreateRejectionEndpoint(allHttpMethods.OrderBy(m => m));
                matches = new List<Endpoint>() { endpoint, };
                edges[new EdgeKey(AnyMethod, false)] = matches;
            }

            return edges
                .Select(kvp => new PolicyNodeEdge(kvp.Key, kvp.Value))
                .ToArray();

            (IReadOnlyList<string> httpMethods, bool acceptCorsPreflight) GetHttpMethods(Endpoint e)
            {
                var metadata = e.Metadata.GetMetadata<IHttpMethodMetadata>();
                return metadata == null ? (Array.Empty<string>(), false) : (metadata.HttpMethods, metadata.AcceptCorsPreflight);
            }
        }

        /// <summary>
        /// For framework use only.
        /// </summary>
        /// <param name="exitDestination"></param>
        /// <param name="edges"></param>
        /// <returns></returns>
        public PolicyJumpTable BuildJumpTable(int exitDestination, IReadOnlyList<PolicyJumpTableEdge> edges)
        {
            var destinations = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var corsPreflightDestinations = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < edges.Count; i++)
            {
                // We create this data, so it's safe to cast it.
                var key = (EdgeKey)edges[i].State;
                if (key.IsCorsPreflightRequest)
                {
                    corsPreflightDestinations.Add(key.HttpMethod, edges[i].Destination);
                }
                else
                {
                    destinations.Add(key.HttpMethod, edges[i].Destination);
                }
            }

            int corsPreflightExitDestination = exitDestination;
            if (corsPreflightDestinations.TryGetValue(AnyMethod, out var matchesAnyVerb))
            {
                // If we have endpoints that match any HTTP method, use that as the exit.
                corsPreflightExitDestination = matchesAnyVerb;
                corsPreflightDestinations.Remove(AnyMethod);
            }

            if (destinations.TryGetValue(AnyMethod, out matchesAnyVerb))
            {
                // If we have endpoints that match any HTTP method, use that as the exit.
                exitDestination = matchesAnyVerb;
                destinations.Remove(AnyMethod);
            }

            return new HttpMethodPolicyJumpTable(
                exitDestination,
                destinations,
                corsPreflightExitDestination,
                corsPreflightDestinations);
        }

        private Endpoint CreateRejectionEndpoint(IEnumerable<string> httpMethods)
        {
            var allow = string.Join(", ", httpMethods);
            return new Endpoint(
                (context) =>
                {
                    context.Response.StatusCode = 405;
                    context.Response.Headers.Add("Allow", allow);
                    return Task.CompletedTask;
                },
                EndpointMetadataCollection.Empty,
                Http405EndpointDisplayName);
        }

        private class HttpMethodPolicyJumpTable : PolicyJumpTable
        {
            private readonly int _exitDestination;
            private readonly Dictionary<string, int> _destinations;
            private readonly int _corsPreflightExitDestination;
            private readonly Dictionary<string, int> _corsPreflightDestinations;

            private readonly bool _supportsCorsPreflight;

            public HttpMethodPolicyJumpTable(
                int exitDestination,
                Dictionary<string, int> destinations,
                int corsPreflightExitDestination,
                Dictionary<string, int> corsPreflightDestinations)
            {
                _exitDestination = exitDestination;
                _destinations = destinations;
                _corsPreflightExitDestination = corsPreflightExitDestination;
                _corsPreflightDestinations = corsPreflightDestinations;

                _supportsCorsPreflight = _corsPreflightDestinations.Count > 0;
            }

            public override int GetDestination(HttpContext httpContext)
            {
                int destination;

                var httpMethod = httpContext.Request.Method;
                if (_supportsCorsPreflight &&
                    string.Equals(httpMethod, PreflightHttpMethod, StringComparison.OrdinalIgnoreCase) &&
                    httpContext.Request.Headers.ContainsKey(OriginHeader) &&
                    httpContext.Request.Headers.TryGetValue(AccessControlRequestMethod, out var accessControlRequestMethod) &&
                    !StringValues.IsNullOrEmpty(accessControlRequestMethod))
                {
                    return _corsPreflightDestinations.TryGetValue(accessControlRequestMethod, out destination)
                        ? destination
                        : _corsPreflightExitDestination;
                }

                return _destinations.TryGetValue(httpMethod, out destination) ? destination : _exitDestination;
            }
        }

        private class HttpMethodMetadataEndpointComparer : EndpointMetadataComparer<IHttpMethodMetadata>
        {
            protected override int CompareMetadata(IHttpMethodMetadata x, IHttpMethodMetadata y)
            {
                // Ignore the metadata if it has an empty list of HTTP methods.
                return base.CompareMetadata(
                    x?.HttpMethods.Count > 0 ? x : null,
                    y?.HttpMethods.Count > 0 ? y : null);
            }
        }

        internal readonly struct EdgeKey : IEquatable<EdgeKey>, IComparable<EdgeKey>, IComparable
        {
            // Note that in contrast with the metadata, the edge represents a possible state change
            // rather than a list of what's allowed. We represent CORS and non-CORS requests as separate
            // states.
            public readonly bool IsCorsPreflightRequest;
            public readonly string HttpMethod;

            public EdgeKey(string httpMethod, bool isCorsPreflightRequest)
            {
                HttpMethod = httpMethod;
                IsCorsPreflightRequest = isCorsPreflightRequest;
            }

            // These are comparable so they can be sorted in tests.
            public int CompareTo(EdgeKey other)
            {
                var compare = HttpMethod.CompareTo(other.HttpMethod);
                if (compare != 0)
                {
                    return compare;
                }

                return IsCorsPreflightRequest.CompareTo(other.IsCorsPreflightRequest);
            }

            public int CompareTo(object obj)
            {
                return CompareTo((EdgeKey)obj);
            }

            public bool Equals(EdgeKey other)
            {
                return
                    IsCorsPreflightRequest == other.IsCorsPreflightRequest &&
                    string.Equals(HttpMethod, other.HttpMethod, StringComparison.OrdinalIgnoreCase);
            }

            public override bool Equals(object obj)
            {
                var other = obj as EdgeKey?;
                return other == null ? false : Equals(other.Value);
            }

            public override int GetHashCode()
            {
                var hash = new HashCodeCombiner();
                hash.Add(IsCorsPreflightRequest ? 1 : 0);
                hash.Add(HttpMethod, StringComparer.Ordinal);
                return hash;
            }

            // Used in GraphViz output.
            public override string ToString()
            {
                return IsCorsPreflightRequest ? $"CORS: {HttpMethod}" : $"HTTP: {HttpMethod}";
            }
        }
    }
}
