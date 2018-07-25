﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Routing.EndpointConstraints;
using Microsoft.AspNetCore.Routing.Matchers;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Routing
{
    [DebuggerDisplay("{DebuggerDisplayString,nq}")]
    public class CompositeEndpointDataSource : EndpointDataSource
    {
        private readonly EndpointDataSource[] _dataSources;
        private readonly object _lock;
        private IReadOnlyList<Endpoint> _endpoints;
        private IChangeToken _consumerChangeToken;
        private CancellationTokenSource _cts;

        internal CompositeEndpointDataSource(IEnumerable<EndpointDataSource> dataSources)
        {
            if (dataSources == null)
            {
                throw new ArgumentNullException(nameof(dataSources));
            }

            CreateChangeToken();
            _dataSources = dataSources.ToArray();
            _lock = new object();
        }

        public override IChangeToken ChangeToken => GetChangeToken();

        public override IChangeToken GetChangeToken()
        {
            EnsureInitialized();
            return _consumerChangeToken;
        }

        public override IReadOnlyList<Endpoint> Endpoints
        {
            get
            {
                EnsureInitialized();
                return _endpoints;
            }
        }

        // Defer initialization to avoid doing lots of reflection on startup.
        private void EnsureInitialized()
        {
            if (_endpoints == null)
            {
                Initialize();
            }
        }

        // Note: we can't use DataSourceDependentCache here because we also need to handle a list of change
        // tokens, which is a complication most of our code doesn't have.
        private void Initialize()
        {
            lock (_lock)
            {
                if (_endpoints == null)
                {
                    _endpoints = _dataSources.SelectMany(d => d.Endpoints).ToArray();

                    foreach (var dataSource in _dataSources)
                    {
                        Extensions.Primitives.ChangeToken.OnChange(
                            dataSource.GetChangeToken,
                            HandleChange);
                    }
                }
            }
        }

        private void HandleChange()
        {
            lock (_lock)
            {
                // Refresh the endpoints from datasource so that callbacks can get the latest endpoints
                _endpoints = _dataSources.SelectMany(d => d.Endpoints).ToArray();

                // Prevent consumers from re-registering callback to inflight events as that can 
                // cause a stackoverflow
                // Example:
                // 1. B registers A
                // 2. A fires event causing B's callback to get called
                // 3. B executes some code in its callback, but needs to re-register callback 
                //    in the same callback
                var oldTokenSource = _cts;
                var oldToken = _consumerChangeToken;

                CreateChangeToken();

                // Raise consumer callbacks. Any new callback registration would happen on the new token
                // created in earlier step.
                oldTokenSource.Cancel();
            }
        }

        private void CreateChangeToken()
        {
            _cts = new CancellationTokenSource();
            _consumerChangeToken = new CancellationChangeToken(_cts.Token);
        }

        private string DebuggerDisplayString
        {
            get
            {
                // Try using private variable '_endpoints' to avoid initialization
                if (_endpoints == null)
                {
                    return "No endpoints";
                }

                var sb = new StringBuilder();
                foreach (var endpoint in _endpoints)
                {
                    if (endpoint is MatcherEndpoint matcherEndpoint)
                    {
                        var template = matcherEndpoint.RoutePattern.RawText;
                        template = string.IsNullOrEmpty(template) ? "\"\"" : template;
                        sb.Append(template);
                        var requiredValues = matcherEndpoint.RequiredValues.Select(kvp => $"{kvp.Key} = \"{kvp.Value ?? "null"}\"");
                        sb.Append(", Required Values: new { ");
                        sb.Append(string.Join(", ", requiredValues));
                        sb.Append(" }");
                        sb.Append(", Order:");
                        sb.Append(matcherEndpoint.Order);

                        var httpEndpointConstraints = matcherEndpoint.Metadata.GetOrderedMetadata<IEndpointConstraintMetadata>()
                            .OfType<HttpMethodEndpointConstraint>();
                        foreach (var constraint in httpEndpointConstraints)
                        {
                            sb.Append(", Http Methods: ");
                            sb.Append(string.Join(", ", constraint.HttpMethods));
                            sb.Append(", Constraint Order:");
                            sb.Append(constraint.Order);
                        }
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.Append("Non-MatcherEndpoint. DisplayName:");
                        sb.AppendLine(endpoint.DisplayName);
                    }
                }
                return sb.ToString();
            }
        }
    }
}
