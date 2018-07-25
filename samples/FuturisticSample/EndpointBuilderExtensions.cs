using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;

namespace FuturisticSample
{
    public static class EndpointBuilderExtensions
    {
        public static EndpointBuilder AddAuthorizationPolicy(this EndpointBuilder builder, string policy)
        {
            builder.Metadata.Add(new AuthorizeAttribute(policy));
            return builder;
        }
    }
}
