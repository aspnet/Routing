using Microsoft.AspNetCore.Authentication;

namespace Microsoft.AspNetCore.Builder
{
    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddManagementPort(this AuthenticationBuilder builder, int port)
        {
            builder.AddScheme<ManagementPortAuthenticationHandlerOptions, ManagementPortAuthenticationHandler>(
                "managementport", 
                "Management Port",
                options => options.Port = port);
            return builder;
        }
    }
}
