﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matchers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace DispatcherSample.Web
{
    /// <summary>
    /// This sets up a sample where a custom EndpointDataSource creates endpoints with MethodInfo based address
    /// and links are generated based on the lookup of this MethodInfo.
    /// Flow of a request is: Dispatcher selects an endpoint => Endpoint middleware is invoked => Selected endpoint
    /// invokes a func which creates an invoker called IHubInvoker which uses the current endpoint's MethodInfo to
    /// instantiate and execute the target method.
    /// </summary>
    public class StartupUsingLinkGenerator
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHubInvoker, DefaultHubInvoker>();

            services.AddRouting();

            services.TryAddEnumerable(ServiceDescriptor.Singleton<EndpointDataSource, HubEndpointDataSource>());

            services.AddDispatcher();
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

            if (endpoint.Address != null &&
                (endpoint.Address.MethodInfo != null ||
                !string.IsNullOrEmpty(endpoint.Address.Name)))
            {
                return InvokeUsingAddressAsync(endpoint.Address, httpContext);
            }

            //todo: non address way of invoking

            return Task.CompletedTask;
        }

        private Task InvokeUsingAddressAsync(Address address, HttpContext httpContext)
        {
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
            else if (!string.IsNullOrEmpty(address.Name))
            {
                //todo
            }

            return Task.CompletedTask;
        }

        private Endpoint GetEndpoint(HttpContext httpContext)
        {
            var endpointFeature = httpContext.Features.Get<IEndpointFeature>();
            if (endpointFeature == null)
            {
                return null;
            }

            return endpointFeature.Endpoint;
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
