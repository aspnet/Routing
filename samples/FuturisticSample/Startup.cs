using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FuturisticSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddHealthChecks();

            #region SHHHHHHH
            services.AddAuthentication().AddManagementPort(int.Parse(Configuration["managementport"]));
            services.AddAuthorizationPolicyEvaluator();
            services.AddAuthorization(options =>
            {
                //HACK: allow anonymous access by default.
                options.DefaultPolicy = null;
                options.AddPolicy("health", b => b.AddPortRequirement().AddAuthenticationSchemes("managementport"));
            });
            #endregion
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseGlobalRouting();

            app.UseStatusCodePages();

            app.UseAuthorization();

            app.UseEndpoint(endpoints =>
            {
                endpoints.AddHealthChecks("/health").AddAuthorizationPolicy("health");
            });
        }
    }
}
