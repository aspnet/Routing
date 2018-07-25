using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matchers;
using Microsoft.AspNetCore.Routing.Patterns;

namespace FuturisticSample
{
    public static class EndpointDataSourceBuilderExtensions
    {
        public static EndpointBuilder AddHealthChecks(this EndpointDataSourceBuilder builder, string template)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var pipeline = builder.New()
                .UseHealthChecks(template)
                .Build();

            var endpoint = new MatcherEndpointBuilder(
                (next) => pipeline,
                RoutePatternFactory.Parse(template),
                new RouteValueDictionary(),
                0);
            endpoint.DisplayName = "Health Checks";

            builder.Endpoints.Add(endpoint);
            return endpoint;
        }
    }
}
