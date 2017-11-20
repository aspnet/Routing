// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Dispatcher.Internal;
using Microsoft.AspNetCore.Dispatcher.Patterns;

#if ROUTING
using Microsoft.AspNetCore.Routing.Template;

namespace Microsoft.AspNetCore.Routing.Tree
#elif DISPATCHER
namespace Microsoft.AspNetCore.Dispatcher
#else
#error
#endif
{
    internal struct TreeEnumerator : IEnumerator<UrlMatchingNode>
    {
        private readonly Stack<UrlMatchingNode> _stack;
        private readonly PathTokenizer _tokenizer;

        public TreeEnumerator(UrlMatchingNode root, PathTokenizer tokenizer)
        {
            _stack = new Stack<UrlMatchingNode>();
            _tokenizer = tokenizer;
            Current = null;

            _stack.Push(root);
        }

        public UrlMatchingNode Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (_stack == null)
            {
                return false;
            }

            while (_stack.Count > 0)
            {
                var next = _stack.Pop();

                // In case of wild card segment, the request path segment length can be greater
                // Example:
                // Template:    a/{*path}
                // Request Url: a/b/c/d
                if (next.IsCatchAll && next.Matches.Count > 0)
                {
                    Current = next;
                    return true;
                }

                // Next template has the same length as the url we are trying to match
                // The only possible matching segments are either our current matches or
                // any catch-all segment after this segment in which the catch all is empty.
                else if (next.Depth >= _tokenizer.Count)
                {
                    if (next.Matches.Count > 0)
                    {
                        Current = next;
                        return true;
                    }
                    else
                    {
                        // We can stop looking as any other child node from this node will be
                        // either a literal, a constrained parameter or a parameter.
                        // (Catch alls and constrained catch alls will show up as candidate matches).
                        continue;
                    }
                }

                if (next.CatchAlls != null)
                {
                    _stack.Push(next.CatchAlls);
                }

                if (next.ConstrainedCatchAlls != null)
                {
                    _stack.Push(next.ConstrainedCatchAlls);
                }

                if (next.Parameters != null)
                {
                    _stack.Push(next.Parameters);
                }

                if (next.ConstrainedParameters != null)
                {
                    _stack.Push(next.ConstrainedParameters);
                }

                if (next.Literals.Count > 0)
                {
                    Debug.Assert(next.Depth < _tokenizer.Count);
                    if (next.Literals.TryGetValue(_tokenizer[next.Depth].Value, out var node))
                    {
                        _stack.Push(node);
                    }
                }
            }

            return false;
        }

        public void Reset()
        {
            _stack.Clear();
            Current = null;
        }

        internal static void AddEntryToTree(UrlMatchingTree tree, InboundRouteEntry entry)
        {
            // The url matching tree represents all the routes asociated with a given
            // order. Each node in the tree represents all the different categories
            // a segment can have for which there is a defined inbound route entry.
            // Each node contains a set of Matches that indicate all the routes for which
            // a URL is a potential match. This list contains the routes with the same
            // number of segments and the routes with the same number of segments plus an
            // additional catch all parameter (as it can be empty).
            // For example, for a set of routes like:
            // 'Customer/Index/{id}'
            // '{Controller}/{Action}/{*parameters}'
            //
            // The route tree will look like:
            // Root ->
            //     Literals: Customer ->
            //                   Literals: Index ->
            //                                Parameters: {id}
            //                                                Matches: 'Customer/Index/{id}'
            //     Parameters: {Controller} ->
            //                     Parameters: {Action} ->
            //                                     Matches: '{Controller}/{Action}/{*parameters}'
            //                                     CatchAlls: {*parameters}
            //                                                    Matches: '{Controller}/{Action}/{*parameters}'
            //
            // When the tree router tries to match a route, it iterates the list of url matching trees
            // in ascending order. For each tree it traverses each node starting from the root in the
            // following order: Literals, constrained parameters, parameters, constrained catch all routes, catch alls.
            // When it gets to a node of the same length as the route its trying to match, it simply looks at the list of
            // candidates (which is in precence order) and tries to match the url against it.

            var current = tree.Root;
#if ROUTING
            var routePattern = entry.RouteTemplate.ToRoutePattern();
            var matcher = new TemplateMatcher(entry.RouteTemplate, entry.Defaults);
#elif DISPATCHER
            var routePattern = entry.RoutePattern;
            var matcher = new RoutePatternMatcher(routePattern, entry.Defaults);
#else
#error
#endif

            for (var i = 0; i < routePattern.PathSegments.Count; i++)
            {
                var segment = routePattern.PathSegments[i];
                if (!segment.IsSimple)
                {
                    // Treat complex segments as a constrained parameter
                    if (current.ConstrainedParameters == null)
                    {
                        current.ConstrainedParameters = new UrlMatchingNode(i + 1);
                    }

                    current = current.ConstrainedParameters;
                    continue;
                }

                Debug.Assert(segment.Parts.Count == 1);
                var part = segment.Parts[0];
                if (part.IsLiteral)
                {
                    var literal = (RoutePatternLiteral)part;
                    if (!current.Literals.TryGetValue(literal.Content, out var next))
                    {
                        next = new UrlMatchingNode(i + 1);
                        current.Literals.Add(literal.Content, next);
                    }

                    current = next;
                    continue;
                }

                // We accept templates that have intermediate optional values, but we ignore
                // those values for route matching. For that reason, we need to add the entry
                // to the list of matches, only if the remaining segments are optional. For example:
                // /{controller}/{action=Index}/{id} will be equivalent to /{controller}/{action}/{id}
                // for the purposes of route matching.
                if (part.IsParameter &&
                    RemainingSegmentsAreOptional(routePattern.PathSegments, i))
                {
#if ROUTING
                    current.Matches.Add(new InboundMatch() { Entry = entry, TemplateMatcher = matcher });
#elif DISPATCHER
                    current.Matches.Add(new InboundMatch() { Entry = entry, RoutePatternMatcher = matcher });
#else
#error
#endif
                }

                var parameter = (RoutePatternParameter)part;
                if (parameter != null && parameter.Constraints.Any() && !parameter.IsCatchAll)
                {
                    if (current.ConstrainedParameters == null)
                    {
                        current.ConstrainedParameters = new UrlMatchingNode(i + 1);
                    }

                    current = current.ConstrainedParameters;
                    continue;
                }

                if (parameter != null && !parameter.IsCatchAll)
                {
                    if (current.Parameters == null)
                    {
                        current.Parameters = new UrlMatchingNode(i + 1);
                    }

                    current = current.Parameters;
                    continue;
                }

                if (parameter != null && parameter.Constraints.Any() && parameter.IsCatchAll)
                {
                    if (current.ConstrainedCatchAlls == null)
                    {
                        current.ConstrainedCatchAlls = new UrlMatchingNode(i + 1) { IsCatchAll = true };
                    }

                    current = current.ConstrainedCatchAlls;
                    continue;
                }

                if (parameter != null && parameter.IsCatchAll)
                {
                    if (current.CatchAlls == null)
                    {
                        current.CatchAlls = new UrlMatchingNode(i + 1) { IsCatchAll = true };
                    }

                    current = current.CatchAlls;
                    continue;
                }

                Debug.Fail("We shouldn't get here.");
            }

#if ROUTING
            current.Matches.Add(new InboundMatch() { Entry = entry, TemplateMatcher = matcher });
            current.Matches.Sort((x, y) =>
            {
                var result = x.Entry.Precedence.CompareTo(y.Entry.Precedence);
                return result == 0 ? x.Entry.RouteTemplate.TemplateText.CompareTo(y.Entry.RouteTemplate.TemplateText) : result;
            });
#elif DISPATCHER
            current.Matches.Add(new InboundMatch() { Entry = entry, RoutePatternMatcher = matcher });
            current.Matches.Sort((x, y) =>
            {
                var result = x.Entry.Precedence.CompareTo(y.Entry.Precedence);
                return result == 0 ? x.Entry.RoutePattern.RawText.CompareTo(y.Entry.RoutePattern.RawText) : result;
            });
#else
#error
#endif

        }
        
        private static bool RemainingSegmentsAreOptional(IReadOnlyList<RoutePatternPathSegment> segments, int currentParameterIndex)
        {
            for (var i = currentParameterIndex; i < segments.Count; i++)
            {
                if (!segments[i].IsSimple)
                {
                    // /{complex}-{segment}
                    return false;
                }

                var part = segments[i].Parts[0];
                if (!part.IsParameter)
                {
                    // /literal
                    return false;
                }

                var parameter = (RoutePatternParameter)part;
                var isOptionalCatchAllOrHasDefaultValue = parameter.IsOptional ||
                    parameter.IsCatchAll ||
                    parameter.DefaultValue != null;

                if (!isOptionalCatchAllOrHasDefaultValue)
                {
                    // /{parameter}
                    return false;
                }
            }

            return true;
        }
    }
}