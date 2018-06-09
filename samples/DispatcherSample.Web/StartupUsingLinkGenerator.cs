// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matchers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DispatcherSample.Web
{
    public class StartupUsingLinkGenerator
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHubInvoker, DefaultHubInvoker>();

            services.AddRouting();

            services.AddDispatcher(options =>
            {
                options.DataSources.Add(new HubEndpointDataSource());
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDispatcher();

            app.UseEndpoint();
        }
    }

    public class HubEndpointDataSource : EndpointDataSource
    {
        private List<MatcherEndpoint> _endpoints;

        public override IChangeToken ChangeToken => NullChangeToken.Singleton;

        public override IReadOnlyList<Endpoint> Endpoints
        {
            get
            {
                if (_endpoints != null)
                {
                    return _endpoints;
                }

                _endpoints = new List<MatcherEndpoint>();

                var hubs = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(type => typeof(IHub).IsAssignableFrom(type));

                RequestDelegate requestDelegate = (httpContext) =>
                {
                    var hubInvoker = httpContext.RequestServices.GetRequiredService<IHubInvoker>();
                    return hubInvoker.InvokeAsync(httpContext);
                };

                foreach (var hub in hubs)
                {
                    foreach (var method in hub.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                    {
                        _endpoints.Add(new MatcherEndpoint(
                            next => requestDelegate,
                            $"/{hub.Name}/{method.Name}", new { hubName = hub.Name, method = method.Name },
                            0,
                            EndpointMetadataCollection.Empty,
                            null,
                            new Address(method)));
                    }
                }

                return _endpoints;
            }
        }
    }

    public interface IHubInvoker
    {
        Task InvokeAsync(HttpContext httpContext);
    }

    public class DefaultHubInvoker : IHubInvoker
    {
        public Task InvokeAsync(HttpContext httpContext)
        {
            var endpoint = GetEndpoint(httpContext);
            if (endpoint == null)
            {
                return Task.CompletedTask;
            }

            var address = endpoint.Address;
            if (address.MethodInfo != null)
            {
                var instance = ActivatorUtilities.CreateInstance(httpContext.RequestServices, address.MethodInfo.DeclaringType);
                var hub = (IHub)instance;
                hub.HttpContext = httpContext;
                hub.LinkGenerator = httpContext.RequestServices.GetRequiredService<ILinkGenerator>();

                var obj = address.MethodInfo.Invoke(instance, parameters: null);
                if (obj is Task task)
                {
                    return task;
                }
            }

            return Task.CompletedTask;
        }

        private MatcherEndpoint GetEndpoint(HttpContext httpContext)
        {
            var endpointFeature = httpContext.Features.Get<IEndpointFeature>();
            if (endpointFeature == null)
            {
                return null;
            }

            return endpointFeature.Endpoint as MatcherEndpoint;
        }
    }

    public class MainHub : IHub
    {
        private readonly ILogger<MainHub> _logger;

        public HttpContext HttpContext { get; set; }

        public ILinkGenerator LinkGenerator { get; set; }

        public MainHub(ILogger<MainHub> logger)
        {
            _logger = logger;
        }

        public Task Index()
        {
            _logger.LogDebug("Inside MainHub.Index");

            var address = new Address(typeof(MainHub).GetMethod(nameof(MainHub.Contact)));
            var link = LinkGenerator.GetLink(new LinkGeneratorContext() { Address = address });
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            HttpContext.Response.ContentType = "text/html";
            return HttpContext.Response.WriteAsync($"<html><body><a href=\"{link}\">Contact</a></body></html>");
        }

        public Task Contact()
        {
            _logger.LogDebug("Inside MainHub.Contact");

            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            return HttpContext.Response.WriteAsync("Hello, World!");
        }
    }

    public interface IHub
    {
        HttpContext HttpContext { get; set; }
        ILinkGenerator LinkGenerator { get; set; }
    }
}
