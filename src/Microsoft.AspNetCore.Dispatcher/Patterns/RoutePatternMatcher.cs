// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Dispatcher.Internal;
using Microsoft.AspNetCore.Dispatcher.Patterns;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Dispatcher
{
    public class RoutePatternMatcher
    {
        private const string SeparatorString = "/";
        private const char SeparatorChar = '/';

        // Perf: This is a cache to avoid looking things up in 'Defaults' each request.
        private readonly bool[] _hasDefaultValue;
        private readonly object[] _defaultValues;

        private static readonly char[] Delimiters = new char[] { SeparatorChar };

        public RoutePatternMatcher(
            RoutePattern routePattern,
            DispatcherValueCollection defaults)
        {
            if (routePattern == null)
            {
                throw new ArgumentNullException(nameof(routePattern));
            }

            RoutePattern = routePattern;
            Defaults = defaults ?? new DispatcherValueCollection();

            // Perf: cache the default value for each parameter (other than complex segments).
            _hasDefaultValue = new bool[RoutePattern.PathSegments.Count];
            _defaultValues = new object[RoutePattern.PathSegments.Count];

            for (var i = 0; i < RoutePattern.PathSegments.Count; i++)
            {
                var segment = RoutePattern.PathSegments[i];
                if (!segment.IsSimple)
                {
                    continue;
                }

                var part = segment.Parts[0];
                if (!part.IsParameter)
                {
                    continue;
                }

                object value;
                var parameter = (RoutePatternParameter)part;
                if (Defaults.TryGetValue(parameter.Name, out value))
                {
                    _hasDefaultValue[i] = true;
                    _defaultValues[i] = value;
                }
            }
        }

        public DispatcherValueCollection Defaults { get; }

        public RoutePattern RoutePattern { get; }

        public bool TryMatch(PathString path, DispatcherValueCollection values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var i = 0;
            var pathTokenizer = new PathTokenizer(path);

            // Perf: We do a traversal of the request-segments + route-segments twice.
            //
            // For most segment-types, we only really need to any work on one of the two passes.
            //
            // On the first pass, we're just looking to see if there's anything that would disqualify us from matching.
            // The most common case would be a literal segment that doesn't match.
            //
            // On the second pass, we're almost certainly going to match the URL, so go ahead and allocate the 'values'
            // and start capturing strings. 
            foreach (var stringSegment in pathTokenizer)
            {
                if (!TryMatchLiterals(i++, stringSegment))
                {
                    return false;
                }
            }

            for (; i < RoutePattern.PathSegments.Count; i++)
            {
                // We've matched the request path so far, but still have remaining route segments. These need
                // to be all single-part parameter segments with default values or else they won't match.
                var pathSegment = RoutePattern.PathSegments[i];
                Debug.Assert(pathSegment != null);

                if (!pathSegment.IsSimple)
                {
                    // If the segment is a complex segment, it MUST contain literals, and we've parsed the full
                    // path so far, so it can't match.
                    return false;
                }

                var part = pathSegment.Parts[0];
                if (part.IsLiteral || part.IsSeparator)
                {
                    // If the segment is a simple literal - which need the URL to provide a value, so we don't match.
                    return false;
                }

                var parameter = (RoutePatternParameter)part;
                if (parameter.IsCatchAll)
                {
                    // Nothing to validate for a catch-all - it can match any string, including the empty string.
                    //
                    // Also, a catch-all has to be the last part, so we're done.
                    break;
                }

                // If we get here, this is a simple segment with a parameter. We need it to be optional, or for the
                // defaults to have a value.
                if (!_hasDefaultValue[i] && !parameter.IsOptional)
                {
                    // There's no default for this (non-optional) parameter so it can't match.
                    return false;
                }
            }

            // At this point we've very likely got a match, so start capturing values for real.
            i = 0;
            foreach (var requestSegment in pathTokenizer)
            {
                var pathSegment = RoutePattern.PathSegments[i++];
                if (SavePathSegmentsAsValues(i, values, requestSegment, pathSegment))
                {
                    break;
                }
                if (!pathSegment.IsSimple)
                {
                    if (!MatchComplexSegment(pathSegment, requestSegment.ToString(), Defaults, values))
                    {
                        return false;
                    }
                }
            }

            for (; i < RoutePattern.PathSegments.Count; i++)
            {
                // We've matched the request path so far, but still have remaining route segments. We already know these
                // are simple parameters that either have a default, or don't need to produce a value.
                var pathSegment = RoutePattern.PathSegments[i];
                Debug.Assert(pathSegment != null);
                Debug.Assert(pathSegment.IsSimple);

                var part = pathSegment.Parts[0];
                Debug.Assert(part.IsParameter);

                // It's ok for a catch-all to produce a null value
                if (part is RoutePatternParameter parameter && (parameter.IsCatchAll || _hasDefaultValue[i]))
                {
                    // Don't replace an existing value with a null.
                    var defaultValue = _defaultValues[i];
                    if (defaultValue != null || !values.ContainsKey(parameter.Name))
                    {
                        values[parameter.Name] = defaultValue;
                    }
                }
            }

            // Copy all remaining default values to the route data
            foreach (var kvp in Defaults)
            {
                if (!values.ContainsKey(kvp.Key))
                {
                    values.Add(kvp.Key, kvp.Value);
                }
            }

            return true;
        }

        private bool TryMatchLiterals(int index, StringSegment stringSegment)
        {
            if (stringSegment.Length == 0)
            {
                return false;
            }

            var pathSegment = RoutePattern.PathSegments[index];
            if (pathSegment == null && stringSegment.Length > 0)
            {
                // If pathSegment is null, then we're out of route segments. All we can match is the empty
                // string.
                return false;
            }
            else if (pathSegment.IsSimple && pathSegment.Parts[0].IsLiteral)
            {
                // This is a literal segment, so we need to match the text, or the route isn't a match.
                var part = pathSegment.Parts[0];
                if (!stringSegment.Equals(part.RawText, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            else if (pathSegment.IsSimple && pathSegment.Parts[0] is RoutePatternParameter parameter && parameter.IsCatchAll)
            {
                // Nothing to validate for a catch-all - it can match any string, including the empty string.
                //
                // Also, a catch-all has to be the last part, so we're done.
            }
            else if (pathSegment.IsSimple && pathSegment.Parts[0].IsParameter)
            {
                // For a parameter, validate that it's a has some length, or we have a default, or it's optional.
                var part = (RoutePatternParameter)pathSegment.Parts[0];
                if (stringSegment.Length == 0 &&
                    !_hasDefaultValue[index] &&
                    !part.IsOptional)
                {
                    // There's no value for this parameter, the route can't match.
                    return false;
                }
            }
            else
            {
                Debug.Assert(!pathSegment.IsSimple);
                // Don't attempt to validate a complex segment at this point other than being non-emtpy,
                // do it in the second pass.
            }
            return true;
        }

        private bool SavePathSegmentsAsValues(int index, DispatcherValueCollection values, StringSegment requestSegment, RoutePatternPathSegment pathSegment)
        {
            if (pathSegment.IsSimple && pathSegment.Parts[0] is RoutePatternParameter parameter && parameter.IsCatchAll)
            {
                // A catch-all captures til the end of the string.
                var captured = requestSegment.Buffer.Substring(requestSegment.Offset);
                if (captured.Length > 0)
                {
                    values[parameter.Name] = captured;
                }
                else
                {
                    // It's ok for a catch-all to produce a null value, so we don't check _hasDefaultValue.
                    values[parameter.Name] = _defaultValues[index];
                }

                // A catch-all has to be the last part, so we're done.
                return true;
            }
            else if (pathSegment.IsSimple && pathSegment.Parts[0].IsParameter)
            {
                // A simple parameter captures the whole segment, or a default value if nothing was
                // provided.
                parameter = (RoutePatternParameter)pathSegment.Parts[0];
                if (requestSegment.Length > 0)
                {
                    values[parameter.Name] = requestSegment.ToString();
                }
                else
                {
                    if (_hasDefaultValue[index])
                    {
                        values[parameter.Name] = _defaultValues[index];
                    }
                }
            }
            return false;
        }

        private bool MatchComplexSegment(
            RoutePatternPathSegment routeSegment,
            string requestSegment,
            IReadOnlyDictionary<string, object> defaults,
            DispatcherValueCollection values)
        {
            var indexOfLastSegment = routeSegment.Parts.Count - 1;

            // We match the request to the template starting at the rightmost parameter
            // If the last segment of template is optional, then request can match the 
            // template with or without the last parameter. So we start with regular matching,
            // but if it doesn't match, we start with next to last parameter. Example:
            // Template: {p1}/{p2}.{p3?}. If the request is one/two.three it will match right away
            // giving p3 value of three. But if the request is one/two, we start matching from the
            // rightmost giving p3 the value of two, then we end up not matching the segment.
            // In this case we start again from p2 to match the request and we succeed giving
            // the value two to p2
            if (routeSegment.Parts[indexOfLastSegment] is RoutePatternParameter parameter && parameter.IsOptional &&
                routeSegment.Parts[indexOfLastSegment - 1].IsSeparator)
            {
                if (MatchComplexSegmentCore(routeSegment, requestSegment, Defaults, values, indexOfLastSegment))
                {
                    return true;
                }
                else
                {
                    if (requestSegment.EndsWith(
                        routeSegment.Parts[indexOfLastSegment - 1].RawText,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    return MatchComplexSegmentCore(
                        routeSegment,
                        requestSegment,
                        Defaults,
                        values,
                        indexOfLastSegment - 2);
                }
            }
            else
            {
                return MatchComplexSegmentCore(routeSegment, requestSegment, Defaults, values, indexOfLastSegment);
            }
        }

        private bool MatchComplexSegmentCore(
            RoutePatternPathSegment routeSegment,
            string requestSegment,
            IReadOnlyDictionary<string, object> defaults,
            DispatcherValueCollection values,
            int indexOfLastSegmentUsed)
        {
            Debug.Assert(routeSegment != null);
            Debug.Assert(routeSegment.Parts.Count > 1);

            // Find last literal segment and get its last index in the string
            var lastIndex = requestSegment.Length;

            RoutePatternParameter parameterNeedsValue = null; // Keeps track of a parameter segment that is pending a value
            RoutePatternPart lastLiteral = null; // Keeps track of the left-most literal we've encountered

            var outValues = new DispatcherValueCollection();

            while (indexOfLastSegmentUsed >= 0)
            {
                var newLastIndex = lastIndex;

                var part = routeSegment.Parts[indexOfLastSegmentUsed];
                if (part.IsParameter)
                {
                    // Hold on to the parameter so that we can fill it in when we locate the next literal
                    parameterNeedsValue = (RoutePatternParameter)part;
                }
                else
                {
                    Debug.Assert(part.IsLiteral || part.IsSeparator);
                    lastLiteral = part;

                    var startIndex = lastIndex - 1;
                    // If we have a pending parameter subsegment, we must leave at least one character for that
                    if (parameterNeedsValue != null)
                    {
                        startIndex--;
                    }

                    if (startIndex < 0)
                    {
                        return false;
                    }

                    var indexOfLiteral = requestSegment.LastIndexOf(
                        part.RawText,
                        startIndex,
                        StringComparison.OrdinalIgnoreCase);
                    if (indexOfLiteral == -1)
                    {
                        // If we couldn't find this literal index, this segment cannot match
                        return false;
                    }

                    // If the first subsegment is a literal, it must match at the right-most extent of the request URI.
                    // Without this check if your route had "/Foo/" we'd match the request URI "/somethingFoo/".
                    // This check is related to the check we do at the very end of this function.
                    if (indexOfLastSegmentUsed == (routeSegment.Parts.Count - 1))
                    {
                        if ((indexOfLiteral + part.RawText.Length) != requestSegment.Length)
                        {
                            return false;
                        }
                    }

                    newLastIndex = indexOfLiteral;
                }

                if ((parameterNeedsValue != null) &&
                    (((lastLiteral != null) && (part.IsLiteral)) || (indexOfLastSegmentUsed == 0)))
                {
                    // If we have a pending parameter that needs a value, grab that value

                    int parameterStartIndex;
                    int parameterTextLength;

                    if (lastLiteral == null)
                    {
                        if (indexOfLastSegmentUsed == 0)
                        {
                            parameterStartIndex = 0;
                        }
                        else
                        {
                            parameterStartIndex = newLastIndex;
                            Debug.Assert(false, "indexOfLastSegementUsed should always be 0 from the check above");
                        }
                        parameterTextLength = lastIndex;
                    }
                    else
                    {
                        // If we're getting a value for a parameter that is somewhere in the middle of the segment
                        if ((indexOfLastSegmentUsed == 0) && (part.IsParameter))
                        {
                            parameterStartIndex = 0;
                            parameterTextLength = lastIndex;
                        }
                        else
                        {
                            parameterStartIndex = newLastIndex + lastLiteral.RawText.Length;
                            parameterTextLength = lastIndex - parameterStartIndex;
                        }
                    }

                    var parameterValueString = requestSegment.Substring(parameterStartIndex, parameterTextLength);

                    if (string.IsNullOrEmpty(parameterValueString))
                    {
                        // If we're here that means we have a segment that contains multiple sub-segments.
                        // For these segments all parameters must have non-empty values. If the parameter
                        // has an empty value it's not a match.                        
                        return false;

                    }
                    else
                    {
                        // If there's a value in the segment for this parameter, use the subsegment value
                        outValues.Add(parameterNeedsValue.Name, parameterValueString);
                    }

                    parameterNeedsValue = null;
                    lastLiteral = null;
                }

                lastIndex = newLastIndex;
                indexOfLastSegmentUsed--;
            }

            // If the last subsegment is a parameter, it's OK that we didn't parse all the way to the left extent of
            // the string since the parameter will have consumed all the remaining text anyway. If the last subsegment
            // is a literal then we *must* have consumed the entire text in that literal. Otherwise we end up matching
            // the route "Foo" to the request URI "somethingFoo". Thus we have to check that we parsed the *entire*
            // request URI in order for it to be a match.
            // This check is related to the check we do earlier in this function for LiteralSubsegments.
            if (lastIndex == 0 || routeSegment.Parts[0].IsParameter)
            {
                foreach (var item in outValues)
                {
                    values.Add(item.Key, item.Value);
                }

                return true;
            }

            return false;
        }
    }
}
