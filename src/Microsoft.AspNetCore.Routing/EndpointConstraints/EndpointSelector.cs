// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Matchers;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Microsoft.AspNetCore.Routing.EndpointConstraints
{
    internal class EndpointSelector
    {
        private static readonly IReadOnlyList<Endpoint> EmptyEndpoints = Array.Empty<Endpoint>();

        private readonly CompositeEndpointDataSource _dataSource;
        private readonly EndpointConstraintCache _endpointConstraintCache;
        private readonly ILogger _logger;

        private Cache _cache;

        public EndpointSelector(
            CompositeEndpointDataSource dataSource,
            EndpointConstraintCache endpointConstraintCache,
            ILoggerFactory loggerFactory)
        {
            _dataSource = dataSource;
            _logger = loggerFactory.CreateLogger<EndpointSelector>();
            _endpointConstraintCache = endpointConstraintCache;
        }

        private Cache Current
        {
            get
            {
                var cache = Volatile.Read(ref _cache);

                if (cache != null)
                {
                    return cache;
                }

                cache = new Cache(_dataSource.Endpoints);
                Volatile.Write(ref _cache, cache);
                return cache;
            }
        }

        public IReadOnlyList<Endpoint> SelectCandidates(RouteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var cache = Current;

            // The Cache works based on a string[] of the route values in a pre-calculated order. This code extracts
            // those values in the correct order.
            var keys = cache.RouteKeys;
            var values = new string[keys.Length];
            for (var i = 0; i < keys.Length; i++)
            {
                context.RouteData.Values.TryGetValue(keys[i], out object value);

                if (value != null)
                {
                    values[i] = value as string ?? Convert.ToString(value);
                }
            }

            if (cache.OrdinalEntries.TryGetValue(values, out var matchingRouteValues) ||
                cache.OrdinalIgnoreCaseEntries.TryGetValue(values, out matchingRouteValues))
            {
                Debug.Assert(matchingRouteValues != null);
                return matchingRouteValues;
            }

            //_logger.NoEndpointsMatched(context.RouteData.Values);
            return EmptyEndpoints;
        }

        public Endpoint SelectBestCandidate(HttpContext context, IReadOnlyList<Endpoint> candidates)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            var finalMatches = EvaluateEndpointConstraints(context, candidates);

            if (finalMatches == null || finalMatches.Count == 0)
            {
                return null;
            }
            else if (finalMatches.Count == 1)
            {
                var selectedEndpoint = finalMatches[0];

                return selectedEndpoint;
            }
            else
            {
                var endpointNames = string.Join(
                    Environment.NewLine,
                    finalMatches.Select(a => a.DisplayName));

                Log.MatchAmbiguous(_logger, context, finalMatches);

                var message = Resources.FormatAmbiguousEndpoints(
                    Environment.NewLine,
                    string.Join(Environment.NewLine, endpointNames));

                throw new AmbiguousMatchException(message);
            }
        }

        private IReadOnlyList<Endpoint> EvaluateEndpointConstraints(
            HttpContext context,
            IReadOnlyList<Endpoint> endpoints)
        {
            var candidates = new List<EndpointSelectorCandidate>();

            // Perf: Avoid allocations
            for (var i = 0; i < endpoints.Count; i++)
            {
                var endpoint = endpoints[i];
                var constraints = _endpointConstraintCache.GetEndpointConstraints(context, endpoint);
                candidates.Add(new EndpointSelectorCandidate(endpoint, constraints));
            }

            var matches = EvaluateEndpointConstraintsCore(context, candidates, startingOrder: null);

            List<Endpoint> results = null;
            if (matches != null)
            {
                results = new List<Endpoint>(matches.Count);
                // Perf: Avoid allocations
                for (var i = 0; i < matches.Count; i++)
                {
                    var candidate = matches[i];
                    results.Add(candidate.Endpoint);
                }
            }

            return results;
        }

        private IReadOnlyList<EndpointSelectorCandidate> EvaluateEndpointConstraintsCore(
            HttpContext context,
            IReadOnlyList<EndpointSelectorCandidate> candidates,
            int? startingOrder)
        {
            // Find the next group of constraints to process. This will be the lowest value of
            // order that is higher than startingOrder.
            int? order = null;

            // Perf: Avoid allocations
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate.Constraints != null)
                {
                    for (var j = 0; j < candidate.Constraints.Count; j++)
                    {
                        var constraint = candidate.Constraints[j];
                        if ((startingOrder == null || constraint.Order > startingOrder) &&
                            (order == null || constraint.Order < order))
                        {
                            order = constraint.Order;
                        }
                    }
                }
            }

            // If we don't find a next then there's nothing left to do.
            if (order == null)
            {
                return candidates;
            }

            // Since we have a constraint to process, bisect the set of endpoints into those with and without a
            // constraint for the current order.
            var endpointsWithConstraint = new List<EndpointSelectorCandidate>();
            var endpointsWithoutConstraint = new List<EndpointSelectorCandidate>();

            var constraintContext = new EndpointConstraintContext();
            constraintContext.Candidates = candidates;
            constraintContext.HttpContext = context;

            // Perf: Avoid allocations
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                var isMatch = true;
                var foundMatchingConstraint = false;

                if (candidate.Constraints != null)
                {
                    constraintContext.CurrentCandidate = candidate;
                    for (var j = 0; j < candidate.Constraints.Count; j++)
                    {
                        var constraint = candidate.Constraints[j];
                        if (constraint.Order == order)
                        {
                            foundMatchingConstraint = true;

                            if (!constraint.Accept(constraintContext))
                            {
                                isMatch = false;
                                //_logger.ConstraintMismatch(
                                //    candidate.Endpoint.DisplayName,
                                //    candidate.Endpoint.Id,
                                //    constraint);
                                break;
                            }
                        }
                    }
                }

                if (isMatch && foundMatchingConstraint)
                {
                    endpointsWithConstraint.Add(candidate);
                }
                else if (isMatch)
                {
                    endpointsWithoutConstraint.Add(candidate);
                }
            }

            // If we have matches with constraints, those are better so try to keep processing those
            if (endpointsWithConstraint.Count > 0)
            {
                var matches = EvaluateEndpointConstraintsCore(context, endpointsWithConstraint, order);
                if (matches?.Count > 0)
                {
                    return matches;
                }
            }

            // If the set of matches with constraints can't work, then process the set without constraints.
            if (endpointsWithoutConstraint.Count == 0)
            {
                return null;
            }
            else
            {
                return EvaluateEndpointConstraintsCore(context, endpointsWithoutConstraint, order);
            }
        }

        // The endpoint selector cache stores a mapping of route-values -> endpoint descriptors for each known set of
        // of route-values. We actually build two of these mappings, one for case-sensitive (fast path) and one for
        // case-insensitive (slow path).
        //
        // This is necessary because MVC routing/endpoint-selection is always case-insensitive. So we're going to build
        // a case-sensitive dictionary that will behave like the a case-insensitive dictionary when you hit one of the
        // canonical entries. When you don't hit a case-sensitive match it will try the case-insensitive dictionary
        // so you still get correct behaviors.
        //
        // The difference here is because while MVC is case-insensitive, doing a case-sensitive comparison is much 
        // faster. We also expect that most of the URLs we process are canonically-cased because they were generated
        // by Url.Endpoint or another routing api.
        //
        // This means that for a set of endpoints like:
        //      { controller = "Home", endpoint = "Index" } -> HomeController::Index1()
        //      { controller = "Home", endpoint = "index" } -> HomeController::Index2()
        //
        // Both of these endpoints match "Index" case-insensitively, but there exist two known canonical casings,
        // so we will create an entry for "Index" and an entry for "index". Both of these entries match **both**
        // endpoints.
        private class Cache
        {
            public Cache(IReadOnlyList<Endpoint> endpoints)
            {
                // We need to store the version so the cache can be invalidated if the endpoints change.
                //Version = endpoints.Version;

                // We need to build two maps for all of the route values.
                OrdinalEntries = new Dictionary<string[], List<Endpoint>>(StringArrayComparer.Ordinal);
                OrdinalIgnoreCaseEntries = new Dictionary<string[], List<Endpoint>>(StringArrayComparer.OrdinalIgnoreCase);

                // We need to first identify of the keys that endpoint selection will look at (in route data). 
                // We want to only consider conventionally routed endpoints here.
                var routeKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < endpoints.Count; i++)
                {
                    var endpoint = endpoints[i];

                    // TODO
                    //if (endpoint.AttributeRouteInfo == null)
                    //{
                    //    // This is a conventionally routed endpoint - so make sure we include its keys in the set of
                    //    // known route value keys.
                    //    foreach (var kvp in endpoint.RouteValues)
                    //    {
                    //        routeKeys.Add(kvp.Key);
                    //    }
                    //}
                }

                // We need to hold on to an ordered set of keys for the route values. We'll use these later to
                // extract the set of route values from an incoming request to compare against our maps of known
                // route values.
                RouteKeys = routeKeys.ToArray();

                // TODO
                //for (var i = 0; i < endpoints.Items.Count; i++)
                //{
                //    var endpoint = endpoints.Items[i];
                //    if (endpoint.AttributeRouteInfo != null)
                //    {
                //        // This only handles conventional routing. Ignore attribute routed endpoints.
                //        continue;
                //    }

                //    // This is a conventionally routed endpoint - so we need to extract the route values associated
                //    // with this endpoint (in order) so we can store them in our dictionaries.
                //    var routeValues = new string[RouteKeys.Length];
                //    for (var j = 0; j < RouteKeys.Length; j++)
                //    {
                //        endpoint.RouteValues.TryGetValue(RouteKeys[j], out routeValues[j]);
                //    }

                //    if (!OrdinalIgnoreCaseEntries.TryGetValue(routeValues, out var entries))
                //    {
                //        entries = new List<Endpoint>();
                //        OrdinalIgnoreCaseEntries.Add(routeValues, entries);
                //    }

                //    entries.Add(endpoint);

                //    // We also want to add the same (as in reference equality) list of endpoints to the ordinal entries.
                //    // We'll keep updating `entries` to include all of the endpoints in the same equivalence class -
                //    // meaning, all conventionally routed endpoints for which the route values are equalignoring case.
                //    //
                //    // `entries` will appear in `OrdinalIgnoreCaseEntries` exactly once and in `OrdinalEntries` once
                //    // for each variation of casing that we've seen.
                //    if (!OrdinalEntries.ContainsKey(routeValues))
                //    {
                //        OrdinalEntries.Add(routeValues, entries);
                //    }
                //}
            }

            public string[] RouteKeys { get; }

            public Dictionary<string[], List<Endpoint>> OrdinalEntries { get; }

            public Dictionary<string[], List<Endpoint>> OrdinalIgnoreCaseEntries { get; }
        }

        private class StringArrayComparer : IEqualityComparer<string[]>
        {
            public static readonly StringArrayComparer Ordinal = new StringArrayComparer(StringComparer.Ordinal);

            public static readonly StringArrayComparer OrdinalIgnoreCase = new StringArrayComparer(StringComparer.OrdinalIgnoreCase);

            private readonly StringComparer _valueComparer;

            private StringArrayComparer(StringComparer valueComparer)
            {
                _valueComparer = valueComparer;
            }

            public bool Equals(string[] x, string[] y)
            {
                if (object.ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x == null ^ y == null)
                {
                    return false;
                }

                if (x.Length != y.Length)
                {
                    return false;
                }

                for (var i = 0; i < x.Length; i++)
                {
                    if (string.IsNullOrEmpty(x[i]) && string.IsNullOrEmpty(y[i]))
                    {
                        continue;
                    }

                    if (!_valueComparer.Equals(x[i], y[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(string[] obj)
            {
                if (obj == null)
                {
                    return 0;
                }

                var hash = new HashCodeCombiner();
                for (var i = 0; i < obj.Length; i++)
                {
                    hash.Add(obj[i], _valueComparer);
                }

                return hash.CombinedHash;
            }
        }

        private static class Log
        {
            private static readonly Action<ILogger, PathString, IEnumerable<string>, Exception> _matchAmbiguous = LoggerMessage.Define<PathString, IEnumerable<string>>(
                LogLevel.Error,
                new EventId(1, "MatchAmbiguous"),
                "Request matched multiple endpoints for request path '{Path}'. Matching endpoints: {AmbiguousEndpoints}");

            public static void MatchAmbiguous(ILogger logger, HttpContext httpContext, IEnumerable<Endpoint> endpoints)
            {
                if (logger.IsEnabled(LogLevel.Error))
                {
                    _matchAmbiguous(logger, httpContext.Request.Path, endpoints.Select(e => e.DisplayName), null);
                }
            }
        }
    }
}