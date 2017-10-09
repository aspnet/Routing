// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Dispatcher
{
    internal static class MatcherLoggerExtensions
    {
        // MatcherBase and HttpMethodEndpointSelector
        private static readonly Action<ILogger, Exception> _servicesInitialized = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(0, "ServicesInitialized"),
            "Services initialized.");

        // MatcherBase
        private static readonly Action<ILogger, Exception> _endpointSelectorsInitialized = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1, "EndpointSelectorsInitialized"),
            "Endpoint selectors were initialized.");

        private static readonly Action<ILogger, string, Exception> _ambiguousEndpoints = LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(3, "AmbiguousEndpoints"),
            "Request matched multiple endpoints resulting in ambiguity. Matching endpoints: {AmbiguousEndpoints}");

        private static readonly Action<ILogger, PathString, Exception> _noEndpointsMatched = LoggerMessage.Define<PathString>(
            LogLevel.Debug,
            new EventId(4, "NoEndpointsMatched"),
            "No endpoints matched the current request path '{PathString}'.");

        // MatcherBase and DispatcherMiddleware
        private static readonly Action<ILogger, string, Exception> _requestShortCircuited = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(2, "RequestShortCircuited"),
            "The current request '{RequestPath}' was short circuited.");

        private static readonly Action<ILogger, string, Exception> _endpointMatched = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(5, "EndpointMatched"),
            "Request matched endpoint '{endpointName}'.");

        // DispatcherMiddleware
        private static readonly Action<ILogger, Type, Exception> _handlerNotCreated = LoggerMessage.Define<Type>(
            LogLevel.Error,
            new EventId(0, "HandlerNotCreated"),
            "A handler could not be created for '{MatcherType}'.");

        //EndpointMiddleware
        private static readonly Action<ILogger, string, Exception> _executingEndpoint = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(0, "ExecutingEndpoint"),
            "Executing endpoint '{EndpointName}'.");

        private static readonly Action<ILogger, string, Exception> _executedEndpoint = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1, "ExecutedEndpoint"),
            "Executed endpoint '{EndpointName}'.");

        // HttpMethodEndpointSelector
        private static readonly Action<ILogger, Exception> _snapshotCreated = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1, "SnapshotCreated"),
            "Snapshot of current endpoints created.");

        private static readonly Action<ILogger, string, Exception> _noHttpMethodFound = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(2, "NoHttpMethodFound"),
            "No HTTP method specified for endpoint '{EndpointName}'.");

        private static readonly Action<ILogger, string, Exception> _endpointAddedAsFallback = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(3, "EndpointAddedAsFallback"),
            "Endpoint '{EndpointName}' added to the list of fallback endpoints.");

        private static readonly Action<ILogger, string, string, Exception> _requestMethodMatchedEndpointMethod = LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(4, "RequestMethodMatchedEndpointMethod"),
            "Request method matched HTTP method '{Method}' for endpoint '{EndpointName}'.");

        private static readonly Action<ILogger, string, string, string, Exception> _requestMethodDidNotMatchEndpointMethod = LoggerMessage.Define<string, string, string>(
            LogLevel.Information,
            new EventId(5, "RequestMethodDidNotMatchEndpointMethod"),
            "Request method '{RequestMethod}' did not match HTTP method '{EndpointMethod}' for endpoint '{EndpointName}'.");

        private static readonly Action<ILogger, string, Exception> _noEndpointMatchedRequestMethod = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(6, "NoEndpointMatchedRequestMethod"),
            "No endpoint matched request method '{Method}'.");

        private static readonly Action<ILogger, Exception> _snapshotRestored = LoggerMessage.Define(
            LogLevel.Information,
            new EventId(7, "SnapshotRestored"),
            "Snapshot of current endpoints restored.");

        private static readonly Action<ILogger, string, Exception> _fallbackAddedAsEndpoint = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(8, "FallbackAddedAsEndpoint"),
            "Fallback '{FallbackName}' added as Endpoint.");

        public static void AmbiguousEndpoints(this ILogger logger, string ambiguousEndpoints)
        {
            _ambiguousEndpoints(logger, ambiguousEndpoints, null);
        }

        public static void EndpointMatched(this ILogger logger, Endpoint endpoint)
        {
            _endpointMatched(logger, endpoint.DisplayName ?? "Unnamed endpoint", null);
        }

        public static void NoEndpointsMatched(this ILogger logger, PathString pathString)
        {
            _noEndpointsMatched(logger, pathString, null);
        }

        public static void EndpointSelectorsInitialized(this ILogger logger)
        {
            _endpointSelectorsInitialized(logger, null);
        }

        public static void ServicesInitialized(this ILogger logger)
        {
            _servicesInitialized(logger, null);
        }

        public static void RequestShortCircuited(this ILogger logger, MatcherContext matcherContext)
        {
            var requestPath = matcherContext.HttpContext.Request.Path;
            _requestShortCircuited(logger, requestPath, null);
        }

        public static void HandlerNotCreated(this ILogger logger, MatcherEntry matcher)
        {
            var matcherType = matcher.GetType();
            _handlerNotCreated(logger, matcherType, null);
        }

        public static void ExecutingEndpoint(this ILogger logger, Endpoint endpoint)
        {
            _executingEndpoint(logger, endpoint.DisplayName ?? "Unnamed endpoint", null);
        }

        public static void ExecutedEndpoint(this ILogger logger, Endpoint endpoint)
        {
            _executedEndpoint(logger, endpoint.DisplayName ?? "Unnamed endpoint", null);
        }

        public static void SnapshotCreated(this ILogger logger)
        {
            _snapshotCreated(logger, null);
        }

        public static void NoHttpMethodFound(this ILogger logger, Endpoint endpoint)
        {
            _noHttpMethodFound(logger, endpoint.DisplayName ?? "Unnamed endpoint", null);
        }

        public static void EndpointAddedAsFallback(this ILogger logger, Endpoint endpoint)
        {
            _endpointAddedAsFallback(logger, endpoint.DisplayName ?? "Unnamed endpoint", null);
        }

        public static void RequestMethodMatchedEndpointMethod(this ILogger logger, string httpMethod, Endpoint endpoint)
        {
            _requestMethodMatchedEndpointMethod(logger, httpMethod, endpoint.DisplayName ?? "Unnamed endpoint", null);
        }

        public static void RequestMethodDidNotMatchEndpointMethod(this ILogger logger, string requestMethod, string endpointMethod, Endpoint endpoint)
        {
            _requestMethodDidNotMatchEndpointMethod(logger, requestMethod, endpointMethod, endpoint.DisplayName ?? "Unnamed endpoint", null);
        }

        public static void NoEndpointMatchedRequestMethod(this ILogger logger, string requestMethod)
        {
            _noEndpointMatchedRequestMethod(logger, requestMethod, null);
        }

        public static void SnapshotRestored(this ILogger logger)
        {
            _snapshotRestored(logger, null);
        }

        public static void FallbackAddedAsEndpoint(this ILogger logger, Endpoint endpoint)
        {
            _fallbackAddedAsEndpoint(logger, endpoint.DisplayName ?? "Unnamed endpoint", null);
        }
    }
}
