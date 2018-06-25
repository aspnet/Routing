// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    internal class DefaultEndpointSelector : EndpointSelector2
    {
        public override Task Select(EndpointSelectorContext context)
        {
            var foundMatch = false;
            for (var i = 0; i < context.Count; i++)
            {
                if (context.IsCandidate(i))
                {
                    if (foundMatch)
                    {
                        ReportAmbiguity(context);

                        // Unreachable, ReportAmbiguityAlways throws
                        throw new NotSupportedException();
                    }

                    foundMatch = true;
                    var endpoint = context.GetEndpoint(i);

                    context.EndpointFeature.Endpoint = endpoint;
                    context.EndpointFeature.Invoker = endpoint.Invoker;
                    context.EndpointFeature.Values = context.GetValues(i);
                }
            }

            return Task.CompletedTask;
        }

        private static void ReportAmbiguity(EndpointSelectorContext context)
        {
            // Build a newline separated list of the endpoint names
            // that are still matches.
            var builder = new StringBuilder();

            for (var i = 0; i < context.Count; i++)
            {
                if (context.IsCandidate(i))
                {
                    builder.Append(context.GetEndpoint(i).DisplayName);
                    builder.Append(Environment.NewLine);
                }
            }

            // Trim extra newline
            builder.Remove(builder.Length - Environment.NewLine.Length, Environment.NewLine.Length);

            var message = Resources.FormatAmbiguousEndpoints(Environment.NewLine, builder.ToString());
            throw new AmbiguousMatchException(message);
        }
    }
}
