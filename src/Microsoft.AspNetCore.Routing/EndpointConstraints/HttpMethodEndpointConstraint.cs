// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Routing.EndpointConstraints
{
    public class HttpMethodEndpointConstraint : IEndpointConstraint
    {
        private readonly string OriginHeader = "Origin";
        private readonly string AccessControlRequestMethod = "Access-Control-Request-Method";
        private readonly string PreflightHttpMethod = "OPTIONS";

        public static readonly int HttpMethodConstraintOrder = 100;

        private readonly IReadOnlyList<string> _httpMethods;

        // Empty collection means any method will be accepted.
        public HttpMethodEndpointConstraint(IEnumerable<string> httpMethods)
        {
            if (httpMethods == null)
            {
                throw new ArgumentNullException(nameof(httpMethods));
            }

            var methods = new List<string>();

            foreach (var method in httpMethods)
            {
                if (string.IsNullOrEmpty(method))
                {
                    throw new ArgumentException("httpMethod cannot be null or empty");
                }

                methods.Add(method);
            }

            _httpMethods = new ReadOnlyCollection<string>(methods);
        }

        public IEnumerable<string> HttpMethods => _httpMethods;

        public int Order => HttpMethodConstraintOrder;

        public virtual bool Accept(EndpointConstraintContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (_httpMethods.Count == 0)
            {
                return true;
            }

            var request = context.HttpContext.Request;
            var method = request.Method;

            if (HttpMethodSupported(method))
            {
                return true;
            }

            // Check if request is a CORS OPTIONS request
            if (string.Equals(request.Method, PreflightHttpMethod, StringComparison.OrdinalIgnoreCase) &&
                request.Headers.ContainsKey(OriginHeader) &&
                request.Headers.TryGetValue(AccessControlRequestMethod, out var accessControlRequestMethod) &&
                !StringValues.IsNullOrEmpty(accessControlRequestMethod))
            {
                if (HttpMethodSupported(accessControlRequestMethod))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HttpMethodSupported(string method)
        {
            for (var i = 0; i < _httpMethods.Count; i++)
            {
                var supportedMethod = _httpMethods[i];
                if (string.Equals(supportedMethod, method, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}