﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Template;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    public sealed class MatcherEndpoint : Endpoint
    {
        internal static readonly Func<RequestDelegate, RequestDelegate> EmptyInvoker = (next) =>
        {
            return (context) => Task.CompletedTask;
        };

        public MatcherEndpoint(
            Func<RequestDelegate, RequestDelegate> invoker,
            string template,
            object values,
            int order,
            EndpointMetadataCollection metadata,
            string displayName,
            Address address)
            : base(metadata, displayName, address)
        {
            if (invoker == null)
            {
                throw new ArgumentNullException(nameof(invoker));
            }

            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            Invoker = invoker;
            Template = template;
            RouteTemplate = TemplateParser.Parse(template);
            Values = new RouteValueDictionary(values);
            Order = order;
        }

        public int Order { get; }
        public Func<RequestDelegate, RequestDelegate> Invoker { get; }
        public string Template { get; }
        public RouteTemplate RouteTemplate { get; }
        public IReadOnlyDictionary<string, object> Values { get; }
    }
}
