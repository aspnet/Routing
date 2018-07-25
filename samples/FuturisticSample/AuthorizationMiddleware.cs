using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Authorization
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IPolicyEvaluator _policyEvaluator;
        private readonly IAuthorizationPolicyProvider _policyProvider;

        public AuthorizationMiddleware(
            RequestDelegate next,
            IPolicyEvaluator policyEvaluator,
            IAuthorizationPolicyProvider policyProvider)
        {
            _next = next;
            _policyEvaluator = policyEvaluator;
            _policyProvider = policyProvider;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var (policy, endpoint) = await GetAuthorizationPolicyAsync(httpContext);
            if (policy == null)
            {
                await _next(httpContext);
                return;
            }

            var authenticateResult = await _policyEvaluator.AuthenticateAsync(policy, httpContext);

            var authorizeResult = await _policyEvaluator.AuthorizeAsync(policy, authenticateResult, httpContext, endpoint);
            if (authorizeResult.Challenged)
            {
                foreach (var scheme in policy.AuthenticationSchemes)
                { 
                    await httpContext.ChallengeAsync(scheme);
                }
                return;
            }
            else if (authorizeResult.Forbidden)
            {
                foreach (var scheme in policy.AuthenticationSchemes)
                {
                    await httpContext.ForbidAsync(scheme);
                }
                return;
            }
            else
            {
                await _next(httpContext);
            }
        }

        private async Task<(AuthorizationPolicy, Endpoint)> GetAuthorizationPolicyAsync(HttpContext httpContext)
        {
            var endpoint = httpContext.Features.Get<IEndpointFeature>().Endpoint;
            var authorizeData = endpoint?.Metadata.GetMetadata<IAuthorizeData>();
            if (authorizeData != null)
            {
                return (await AuthorizationPolicy.CombineAsync(_policyProvider, new[] { authorizeData }), endpoint);
            }

            return (await _policyProvider.GetDefaultPolicyAsync(), endpoint);
        }
    }
}
