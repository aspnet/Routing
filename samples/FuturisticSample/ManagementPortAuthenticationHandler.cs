
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication
{
    public class ManagementPortAuthenticationHandler : AuthenticationHandler<ManagementPortAuthenticationHandlerOptions>
    {
        public ManagementPortAuthenticationHandler(IOptionsMonitor<ManagementPortAuthenticationHandlerOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) 
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Context.Connection.LocalPort == Options.Port)
            {
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(CreatePrincipal(), Scheme.Name)));
            }

            return Task.FromResult(AuthenticateResult.NoResult());
        }

        private ClaimsPrincipal CreatePrincipal()
        {
            var identity = new ClaimsIdentity((IEnumerable<Claim>)new Claim[] { new Claim("managementport", "true"), }, Scheme.Name);
            return new ClaimsPrincipal(identity);
        }
    }

    public class ManagementPortAuthenticationHandlerOptions : AuthenticationSchemeOptions
    {
        public int Port { get; set; }
    }
}
