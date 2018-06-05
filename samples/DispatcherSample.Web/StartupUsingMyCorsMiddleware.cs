using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matchers;
using Microsoft.Extensions.DependencyInjection;

namespace DispatcherSample.Web
{
    public class StartupUsingMyCorsMiddleware
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ICorsPolicyProvider, DefaultCorsPolicyProvider>();

            services.AddSingleton<ICorsService, DefaultCorsService>();

            services.AddRouting();

            services.AddDispatcher(options =>
            {
                options.DataSources.Add(new DefaultEndpointDataSource(new[]
                {
                    // Ex: Controller having DisableCors and action having EnableCors
                    new MatcherEndpoint((next) => (httpContext) =>
                        {
                            var response = httpContext.Response;
                            response.StatusCode = 200;
                            response.ContentType = "text/plain";
                            return response.WriteAsync("EndpointWithEnableCors");
                        },
                        "/enableCors", new { }, 0, new EndpointMetadataCollection(new object[]{ new DisableCorsAttribute(), new EnableCorsAttribute() }), "EndpointWithEnableCors"),
                    // Ex: Controller having EnableCors and action having DisableCors
                    new MatcherEndpoint((next) => (httpContext) =>
                        {
                            var response = httpContext.Response;
                            response.StatusCode = 200;
                            response.ContentType = "text/plain";
                            return response.WriteAsync("EndpointWithDisableCors");
                        },
                        "/disableCors", new { }, 0, new EndpointMetadataCollection(new object[]{ new EnableCorsAttribute(), new DisableCorsAttribute() }), "EndpointWithDisableCors"),
                    // Ex: No explicit decoration of Cors metadata attributes on controller/action
                    new MatcherEndpoint((next) => (httpContext) =>
                        {
                            var response = httpContext.Response;
                            response.StatusCode = 200;
                            response.ContentType = "text/plain";
                            return response.WriteAsync("EndpointWithNoExplicitCorsMetadata");
                        },
                        "/noExplicitCorsMetadata", new { }, 0, EndpointMetadataCollection.Empty, "EndpointWithNoExplicitCorsMetadata"),
                    new MatcherEndpoint((next) => (httpContext) =>
                        {
                            var response = httpContext.Response;
                            response.StatusCode = 200;
                            response.ContentType = "text/plain";
                            return response.WriteAsync("Default endpoint");
                        },
                        "/", new { }, 0, EndpointMetadataCollection.Empty, "DefaultEndpoint"),
                }));
            });

            services.Configure<CorsOptions>(o =>
            {
                o.AddDefaultPolicy(cpb =>
                {
                    cpb
                    .WithOrigins("http://foo.com");
                });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDispatcher();

            app.UseMiddleware<MyCorsMiddleware>();

            app.UseEndpoint();
        }
    }
}
