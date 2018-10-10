// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;

namespace RoutingSample.Web.DomainPolicy
{
    internal class DomainMatcherPolicy : MatcherPolicy, IEndpointComparerPolicy, INodeBuilderPolicy
    {
        // Run after HTTP methods, but before 'default'.
        public override int Order { get; } = -100;

        public IComparer<Endpoint> Comparer { get; } = new DomainMetadataEndpointComparer();

        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            return endpoints.Any(e => e.Metadata.GetMetadata<DomainMetadata>()?.Domains.Count > 0);
        }

        public IReadOnlyList<PolicyNodeEdge> GetEdges(IReadOnlyList<Endpoint> endpoints)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            // The algorithm here is designed to be preserve the order of the endpoints
            // while also being relatively simple. Preserving order is important.

            // First, build a dictionary of all of the domains that are included
            // at this node.
            //
            // For now we're just building up the set of keys. We don't add any endpoints
            // to lists now because we don't want ordering problems.
            var edges = new Dictionary<HostString, List<Endpoint>>();
            for (var i = 0; i < endpoints.Count; i++)
            {
                var endpoint = endpoints[i];
                var domains = endpoint.Metadata.GetMetadata<DomainMetadata>()?.Domains;
                if (domains == null || domains.Count == 0)
                {
                    domains = new[] { new HostString() };
                }

                for (var j = 0; j < domains.Count; j++)
                {
                    var contentType = domains[j];

                    if (!edges.ContainsKey(contentType))
                    {
                        edges.Add(contentType, new List<Endpoint>());
                    }
                }
            }

            // Now in a second loop, add endpoints to these lists. We've enumerated all of
            // the states, so we want to see which states this endpoint matches.
            for (var i = 0; i < endpoints.Count; i++)
            {
                var endpoint = endpoints[i];
                var domains = endpoint.Metadata.GetMetadata<DomainMetadata>()?.Domains ?? Array.Empty<HostString>();
                if (domains.Count == 0)
                {
                    // OK this means that this endpoint matches *all* domains.
                    // So, loop and add it to all states.
                    foreach (var kvp in edges)
                    {
                        kvp.Value.Add(endpoint);
                    }
                }
                else
                {
                    // OK this endpoint matches specific domains
                    foreach (var kvp in edges)
                    {
                        // The edgeKey maps to a possible request header value
                        var edgeKey = kvp.Key;

                        for (var j = 0; j < domains.Count; j++)
                        {
                            var domain = domains[j];

                            if (edgeKey == domain)
                            {
                                kvp.Value.Add(endpoint);
                                break;
                            }
                        }
                    }
                }
            }

            return edges
                .Select(kvp => new PolicyNodeEdge(kvp.Key, kvp.Value))
                .ToArray();
        }

        public PolicyJumpTable BuildJumpTable(int exitDestination, IReadOnlyList<PolicyJumpTableEdge> edges)
        {
            if (edges == null)
            {
                throw new ArgumentNullException(nameof(edges));
            }

            // Since our 'edges' can have wildcards, we do a sort based on how wildcard-ey they
            // are then then execute them in linear order.
            var ordered = edges
                .Select(e => (domain: (HostString)e.State, destination: e.Destination))
                .OrderBy(e => GetScore(e.domain))
                .ToArray();

            return new DomainPolicyJumpTable(exitDestination, ordered);
        }

        private int GetScore(in HostString domain)
        {
            // Higher score == lower priority.
            if (!string.IsNullOrEmpty(domain.Host) && domain.Port != null)
            {
                return 1; // Has host AND port
            }
            else if (!string.IsNullOrEmpty(domain.Host) || domain.Port != null)
            {
                return 2; // Has host OR port
            }
            else
            {
                return 3; // Has neither
            }
        }

        private class DomainMetadataEndpointComparer : EndpointMetadataComparer<DomainMetadata>
        {
            protected override int CompareMetadata(DomainMetadata x, DomainMetadata y)
            {
                // Ignore the metadata if it has an empty list of IP endpoint types.
                return base.CompareMetadata(
                    x?.Domains.Count > 0 ? x : null,
                    y?.Domains.Count > 0 ? y : null);
            }
        }

        private class DomainPolicyJumpTable : PolicyJumpTable
        {
            private (HostString domain, int destination)[] _destinations;
            private int _exitDestination;

            public DomainPolicyJumpTable(int exitDestination, (HostString domain, int destination)[] destinations)
            {
                _exitDestination = exitDestination;
                _destinations = destinations;
            }

            public override int GetDestination(HttpContext httpContext)
            {
                var destinations = _destinations;
                for (var i = 0; i < destinations.Length; i++)
                {
                    if ((string.IsNullOrEmpty(destinations[i].domain.Host) ||
                        httpContext.Request.Host.Host == destinations[i].domain.Host) &&
                        (destinations[i].domain.Port == null ||
                        httpContext.Request.Host.Port == destinations[i].domain.Port))
                    {
                        return destinations[i].destination;
                    }
                }

                return _exitDestination;
            }
        }
    }
}
