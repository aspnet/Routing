using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace DispatcherSample.Web
{
    public class MyCorsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ICorsService _corsService;
        private readonly ICorsPolicyProvider _corsPolicyProvider;
        private readonly IOptions<CorsOptions> _options;

        public MyCorsMiddleware(
            RequestDelegate next,
            ICorsService corsService,
            ICorsPolicyProvider corsPolicyProvider,
            IOptions<CorsOptions> options)
        {
            _next = next;
            _corsService = corsService;
            _corsPolicyProvider = corsPolicyProvider;
            _options = options;
        }

        public Task Invoke(HttpContext httpContext)
        {
            if (IsCorsEnabled(httpContext))
            {
                return InvokeInternal(httpContext);
            }

            return _next(httpContext);
        }

        private async Task InvokeInternal(HttpContext httpContext)
        {
            var corsPolicy = await GetCorsPolicyAsync(httpContext);
            var corsResult = _corsService.EvaluatePolicy(httpContext, corsPolicy);
            _corsService.ApplyResult(corsResult, httpContext.Response);

            var accessControlRequestMethod = httpContext.Request.Headers[HeaderNames.AccessControlRequestMethod];
            if (string.Equals(
                    httpContext.Request.Method,
                    HttpMethods.Options,
                    StringComparison.OrdinalIgnoreCase) &&
                    !StringValues.IsNullOrEmpty(accessControlRequestMethod))
            {
                // Since there is a policy which was identified,
                // always respond to preflight requests.
                httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }

            await _next(httpContext);
        }

        private bool IsCorsEnabled(HttpContext httpContext)
        {
            var endpointFeature = httpContext.Features.Get<IEndpointFeature>();
            if (endpointFeature != null)
            {
                var metadataCollection = endpointFeature.Endpoint.Metadata;
                if (metadataCollection.Count > 0)
                {
                    for (var i = metadataCollection.Count - 1; i >= 0; i--)
                    {
                        var metadata = metadataCollection[i];
                        if (metadata is IDisableCorsAttribute)
                        {
                            return false;
                        }
                        else if (metadata is IEnableCorsAttribute)
                        {
                            return true;
                        }
                    }
                }
            }

            return true;
        }

        private async Task<CorsPolicy> GetCorsPolicyAsync(HttpContext httpContext)
        {
            string policyName = null;

            var endpointFeature = httpContext.Features.Get<IEndpointFeature>();
            if (endpointFeature != null)
            {
                var metadata = endpointFeature.Endpoint.Metadata;
                var enableCors = metadata.GetMetadata<IEnableCorsAttribute>();
                if (enableCors != null)
                {
                    policyName = enableCors.PolicyName;
                }
            }

            return await _corsPolicyProvider.GetPolicyAsync(httpContext, policyName);
        }
    }
}
