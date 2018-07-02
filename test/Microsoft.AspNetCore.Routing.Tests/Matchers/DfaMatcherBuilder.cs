// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Routing.EndpointConstraints;
using Microsoft.AspNetCore.Routing.Template;
using static Microsoft.AspNetCore.Routing.Matchers.DfaMatcher;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal class DfaMatcherBuilder : MatcherBuilder
    {
        private List<Entry> _entries = new List<Entry>();

        public override void AddEndpoint(MatcherEndpoint endpoint)
        {
            var parsed = TemplateParser.Parse(endpoint.Template);
            _entries.Add(new Entry()
            {
                HttpMethod = endpoint.Metadata.OfType<HttpMethodEndpointConstraint>().FirstOrDefault()?.HttpMethods.Single(),
                Order = 0,
                Pattern = parsed,
                Precedence = RoutePrecedence.ComputeInbound(parsed),
                Endpoint = endpoint,
            });
        }

        public override Matcher Build()
        {
            Sort(_entries);

            var root = new Node() { Depth = -1 };

            // We build the tree by doing a BFS over the list of entries. This is important
            // because a 'parameter' node can also traverse the same paths that literal nodes traverse.
            var maxDepth = 0;
            for (var i = 0; i < _entries.Count; i++)
            {
                maxDepth = Math.Max(maxDepth, _entries[i].Pattern.Segments.Count);
            }

            for (var depth = 0; depth <= maxDepth; depth++)
            {
                for (var i = 0; i < _entries.Count; i++)
                {
                    var entry = _entries[i];
                    if (entry.Pattern.Segments.Count < depth)
                    {
                        continue;
                    }

                    // Find the parents of this edge at the current depth
                    var parents = new List<Node>() { root };
                    for (var j = 0; j < depth; j++)
                    {
                        var next = new List<Node>();
                        for (var k = 0; k < parents.Count; k++)
                        {
                            next.Add(Traverse(parents[k], entry.Pattern.Segments[j]));
                        }

                        parents = next;
                    }

                    if (entry.Pattern.Segments.Count == depth)
                    {
                        for (var j = 0; j < parents.Count; j++)
                        {
                            var parent = parents[j];
                            parent.Matches.Add(entry);
                        }

                        continue;
                    }

                    var segment = entry.Pattern.Segments[depth];
                    for (var j = 0; j < parents.Count; j++)
                    {
                        var parent = parents[j];
                        if (segment.IsSimple && segment.Parts[0].IsLiteral)
                        {
                            if (!parent.Literals.TryGetValue(segment.Parts[0].Text, out var next))
                            {
                                next = new Node() { Depth = depth, };
                                parent.Literals.Add(segment.Parts[0].Text, next);
                            }
                        }
                        else if (segment.IsSimple && segment.Parts[0].IsParameter)
                        {
                            if (!parent.Literals.TryGetValue("*", out var next))
                            {
                                next = new Node() { Depth = depth, };
                                parent.Literals.Add("*", next);
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("We only support simple segments.");
                        }
                    }
                }
            }

            var states = new List<State>();
            var tables = new List<JumpTableBuilder>();
            AddNode(root, states, tables);

            var exit = states.Count;
            states.Add(new State()
            {
                Candidates = Array.Empty<Candidate>(),
                CandidateIndices = Array.Empty<int>(),
                CandidateGroups = Array.Empty<int>(),
            });
            tables.Add(new JumpTableBuilder() { Exit = exit, });

            for (var i = 0; i < tables.Count; i++)
            {
                if (tables[i].Default == -1)
                {
                    tables[i].Default = exit;
                }

                if (tables[i].Exit == -1)
                {
                    tables[i].Exit = exit;
                }
            }

            for (var i = 0; i < states.Count; i++)
            {
                states[i] = new State()
                {
                    Candidates = states[i].Candidates,
                    CandidateIndices = states[i].CandidateIndices,
                    CandidateGroups = states[i].CandidateGroups,
                    Transitions = tables[i].Build(),
                };
            }

            return new DfaMatcher(states.ToArray());
        }

        private Node Traverse(Node node, TemplateSegment segment)
        {
            if (!segment.IsSimple)
            {
                throw new InvalidOperationException("We only support simple segments.");
            }

            if (segment.Parts[0].IsLiteral)
            {
                return node.Literals[segment.Parts[0].Text];
            }

            return node.Literals["*"];
        }

        private static int AddNode(Node node, List<State> states, List<JumpTableBuilder> tables)
        {
            Sort(node.Matches);

            var index = states.Count;
            states.Add(new State()
            {
                Candidates = node.Matches.Select(CreateCandidate).ToArray(),
                CandidateIndices = Enumerable.Range(0, node.Matches.Count).ToArray(),
                CandidateGroups = CreateCandidateGroups(node),
            });

            var table = new JumpTableBuilder();
            tables.Add(table);

            foreach (var kvp in node.Literals)
            {
                if (kvp.Key == "*")
                {
                    continue;
                }

                var transition = AddNode(kvp.Value, states, tables);
                table.AddEntry(kvp.Key, transition);
            }

            var defaultIndex = -1;
            if (node.Literals.TryGetValue("*", out var exit))
            {
                defaultIndex = AddNode(exit, states, tables);
            }

            table.Default = defaultIndex;
            return index;
        }

        private static Candidate CreateCandidate(Entry entry)
        {
            var processors = new List<MatchProcessor>();
            for (var i = 0; i < entry.Pattern.Segments.Count; i++)
            {
                var segment = entry.Pattern.Segments[i];
                if (segment.IsSimple && segment.Parts[0].IsParameter)
                {
                    processors.Add(new ParameterSegmentMatchProcessor(i, segment.Parts[0].Name));
                }
            }

            return new Candidate(entry.Endpoint, processors.ToArray());
        }

        private static int[] CreateCandidateGroups(Node node)
        {
            if (node.Matches.Count == 0)
            {
                return Array.Empty<int>();
            }

            var groups = new List<int>();

            var order = node.Matches[0].Order;
            var precedence = node.Matches[0].Precedence;
            var httpMethodScore = GetHttpMethodScore(node.Matches[0].Endpoint);
            var length = 1;

            for (var i = 1; i < node.Matches.Count; i++)
            {
                if (node.Matches[i].Order != order ||
                    node.Matches[i].Precedence != precedence ||
                    GetHttpMethodScore(node.Matches[i].Endpoint) != httpMethodScore)
                {
                    groups.Add(length);
                    length = 0;
                }

                length++;
            }

            groups.Add(length);

            return groups.ToArray();

            int GetHttpMethodScore(Endpoint endpoint)
            {
                if (endpoint.Metadata.OfType<HttpMethodEndpointConstraint>().Any())
                {
                    return 1;
                }

                return 0;
            }
        }

        private static void Sort(List<Entry> entries)
        {
            entries.Sort((x, y) =>
            {
                var comparison = x.Order.CompareTo(y.Order);
                if (comparison != 0)
                {
                    return comparison;
                }

                comparison = x.Precedence.CompareTo(y.Precedence);
                if (comparison != 0)
                {
                    return comparison;
                }

                if (x.HttpMethod != null && y.HttpMethod == null)
                {
                    return 1.CompareTo(0);
                }
                else if (x.HttpMethod == null && y.HttpMethod == null)
                {
                    return 0.CompareTo(1);
                }

                return x.Pattern.TemplateText.CompareTo(y.Pattern.TemplateText);
            });
        }

        private static Node DeepCopy(Node node)
        {
            var copy = new Node() { Depth = node.Depth, };
            copy.Matches.AddRange(node.Matches);

            foreach (var kvp in node.Literals)
            {
                copy.Literals.Add(kvp.Key, DeepCopy(kvp.Value));
            }

            return node;
        }

        private class Entry
        {
            public int Order;
            public decimal Precedence;
            public string HttpMethod;
            public RouteTemplate Pattern;
            public Endpoint Endpoint;
        }

        [DebuggerDisplay("{DebuggerToString(),nq}")]
        private class Node
        {
            public int Depth { get; set; }

            public List<Entry> Matches { get; } = new List<Entry>();

            public Dictionary<string, Node> Literals { get; } = new Dictionary<string, Node>(StringComparer.OrdinalIgnoreCase);

            private string DebuggerToString()
            {
                var builder = new StringBuilder();
                builder.Append("d:");
                builder.Append(Depth);
                builder.Append(" m:");
                builder.Append(Matches.Count);
                builder.Append(" c: ");
                builder.Append(string.Join(", ", Literals.Select(kvp => $"{kvp.Key}->({kvp.Value.DebuggerToString()})")));
                return builder.ToString();
            }
        }
    }
}
