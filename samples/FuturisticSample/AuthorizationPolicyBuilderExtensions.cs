using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Microsoft.AspNetCore.Builder
{
    public static class AuthorizationPolicyBuilderExtensions
    {
        public static AuthorizationPolicyBuilder AddPortRequirement(this AuthorizationPolicyBuilder builder)
        {
            return
                builder
                .AddAuthenticationSchemes("managementport")
                .AddRequirements(new AssertionRequirement(c => c.User.HasClaim("managementport", "true")));
        }
    }
}
