// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace RoutingSandbox
{
    public class UseEndpointRoutingStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();

            services.AddMvc();

            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = new PathString("/login");
                    options.AccessDeniedPath = new PathString("/forbidden");
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Members", p => p.RequireAuthenticatedUser());
                options.AddPolicy("Admins", p => p.RequireRole("admin"));

                options.DefaultPolicy = options.GetPolicy("Members");
            });
            services.AddAuthorizationPolicyEvaluator();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseEndpointRouting(builder =>
            {
                builder.MapHello("/helloworld", "World");

                builder.MapHello("/helloworld-secret", "Secret World")
                    .RequireAuthorization(new AuthorizeAttribute("Admins"));

                builder.MapRazorPages();

                builder.MapGet(
                    "/",
                    (httpContext) =>
                    {
                        var dataSource = httpContext.RequestServices.GetRequiredService<EndpointDataSource>();

                        var sb = new StringBuilder();
                        sb.AppendLine("<html><body>");
                        sb.AppendLine("<h1>Endpoints:</h1>");
                        sb.AppendLine("<ul>");
                        foreach (var endpoint in dataSource.Endpoints.OfType<RouteEndpoint>().OrderBy(e => e.RoutePattern.RawText, StringComparer.OrdinalIgnoreCase))
                        {
                            sb.AppendLine($@"<li><a href=""{endpoint.RoutePattern.RawText}"">{endpoint.RoutePattern.RawText}</a></li>");
                        }
                        sb.AppendLine("</ul>");
                        if (httpContext.User.Identity.IsAuthenticated)
                        {
                            sb.AppendLine("<h1>Authenticated</h1>");
                            sb.AppendLine("<ul>");
                            foreach (var claim in httpContext.User.Claims)
                            {
                                sb.AppendLine($"<li>{claim}</li>");
                            }
                            sb.AppendLine("</ul>");
                        }
                        else
                        {
                            sb.AppendLine("<h1>Unauthenticated</h1>");
                        }
                        sb.AppendLine("</body></html>");

                        var response = httpContext.Response;
                        response.StatusCode = 200;
                        response.ContentType = "text/html";
                        return response.WriteAsync(sb.ToString());
                    });
                builder.MapGet(
                    "/forbidden",
                    (httpContext) =>
                    {
                        var response = httpContext.Response;
                        response.StatusCode = 200;
                        response.ContentType = "text/plain";
                        return response.WriteAsync("Forbidden endpoint!");
                    });
                builder.MapGet(
                    "/graph",
                    "DFA Graph",
                    (httpContext) =>
                    {
                        using (var writer = new StreamWriter(httpContext.Response.Body, Encoding.UTF8, 1024, leaveOpen: true))
                        {
                            var graphWriter = httpContext.RequestServices.GetRequiredService<DfaGraphWriter>();
                            var dataSource = httpContext.RequestServices.GetRequiredService<EndpointDataSource>();
                            graphWriter.Write(dataSource, writer);
                        }

                        return Task.CompletedTask;
                    });
            });

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoint();
        }
    }
}
