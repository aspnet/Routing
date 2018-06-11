// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Microsoft.AspNetCore.Routing.EndpointConstraints
{
    public class EndpointConstraintContext
    {
        public IReadOnlyList<EndpointSelectorCandidate> Candidates { get; set; }

        public EndpointSelectorCandidate CurrentCandidate { get; set; }

        public HttpContext HttpContext { get; set; }
    }

    public interface IEndpointConstraint : IEndpointConstraintMetadata
    {
        int Order { get; }

        bool Accept(EndpointConstraintContext context);
    }

    public interface IEndpointConstraintMetadata
    {
    }

    public struct EndpointSelectorCandidate
    {
        public EndpointSelectorCandidate(Endpoint endpoint, IReadOnlyList<IEndpointConstraint> constraints)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            Endpoint = endpoint;
            Constraints = constraints;
        }

        public Endpoint Endpoint { get; }

        public IReadOnlyList<IEndpointConstraint> Constraints { get; }
    }

    public class EndpointConstraintCache
    {
        private readonly CompositeEndpointDataSource _dataSource;
        private readonly IEndpointConstraintProvider[] _endpointConstraintProviders;

        private volatile InnerCache _currentCache;

        public EndpointConstraintCache(
            CompositeEndpointDataSource dataSource,
            IEnumerable<IEndpointConstraintProvider> endpointConstraintProviders)
        {
            _dataSource = dataSource;
            _endpointConstraintProviders = endpointConstraintProviders.OrderBy(item => item.Order).ToArray();
        }

        private InnerCache CurrentCache
        {
            get
            {
                var current = _currentCache;
                var endpointDescriptors = _dataSource.Endpoints;

                if (current == null)
                {
                    current = new InnerCache();
                    _currentCache = current;
                }

                return current;
            }
        }

        public IReadOnlyList<IEndpointConstraint> GetEndpointConstraints(HttpContext httpContext, Endpoint endpoint)
        {
            var cache = CurrentCache;

            if (cache.Entries.TryGetValue(endpoint, out var entry))
            {
                return GetEndpointConstraintsFromEntry(entry, httpContext, endpoint);
            }

            if (endpoint.Metadata == null || endpoint.Metadata.Count == 0)
            {
                return null;
            }

            var items = endpoint.Metadata
                .OfType<IEndpointConstraintMetadata>()
                .Select(m => new EndpointConstraintItem(m))
                .ToList();

            ExecuteProviders(httpContext, endpoint, items);

            var endpointConstraints = ExtractEndpointConstraints(items);

            var allEndpointConstraintsCached = true;
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (!item.IsReusable)
                {
                    item.Constraint = null;
                    allEndpointConstraintsCached = false;
                }
            }

            if (allEndpointConstraintsCached)
            {
                entry = new CacheEntry(endpointConstraints);
            }
            else
            {
                entry = new CacheEntry(items);
            }

            cache.Entries.TryAdd(endpoint, entry);
            return endpointConstraints;
        }

        private IReadOnlyList<IEndpointConstraint> GetEndpointConstraintsFromEntry(CacheEntry entry, HttpContext httpContext, Endpoint endpoint)
        {
            Debug.Assert(entry.EndpointConstraints != null || entry.Items != null);

            if (entry.EndpointConstraints != null)
            {
                return entry.EndpointConstraints;
            }

            var items = new List<EndpointConstraintItem>(entry.Items.Count);
            for (var i = 0; i < entry.Items.Count; i++)
            {
                var item = entry.Items[i];
                if (item.IsReusable)
                {
                    items.Add(item);
                }
                else
                {
                    items.Add(new EndpointConstraintItem(item.Metadata));
                }
            }

            ExecuteProviders(httpContext, endpoint, items);

            return ExtractEndpointConstraints(items);
        }

        private void ExecuteProviders(HttpContext httpContext, Endpoint endpoint, List<EndpointConstraintItem> items)
        {
            var context = new EndpointConstraintProviderContext(httpContext, endpoint, items);

            for (var i = 0; i < _endpointConstraintProviders.Length; i++)
            {
                _endpointConstraintProviders[i].OnProvidersExecuting(context);
            }

            for (var i = _endpointConstraintProviders.Length - 1; i >= 0; i--)
            {
                _endpointConstraintProviders[i].OnProvidersExecuted(context);
            }
        }

        private IReadOnlyList<IEndpointConstraint> ExtractEndpointConstraints(List<EndpointConstraintItem> items)
        {
            var count = 0;
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].Constraint != null)
                {
                    count++;
                }
            }

            if (count == 0)
            {
                return null;
            }

            var endpointConstraints = new IEndpointConstraint[count];
            var endpointConstraintIndex = 0;
            for (int i = 0; i < items.Count; i++)
            {
                var endpointConstraint = items[i].Constraint;
                if (endpointConstraint != null)
                {
                    endpointConstraints[endpointConstraintIndex++] = endpointConstraint;
                }
            }

            return endpointConstraints;
        }

        private class InnerCache
        {
            public InnerCache()
            {
            }

            public ConcurrentDictionary<Endpoint, CacheEntry> Entries { get; } =
                new ConcurrentDictionary<Endpoint, CacheEntry>();
        }

        private struct CacheEntry
        {
            public CacheEntry(IReadOnlyList<IEndpointConstraint> endpointConstraints)
            {
                EndpointConstraints = endpointConstraints;
                Items = null;
            }

            public CacheEntry(List<EndpointConstraintItem> items)
            {
                Items = items;
                EndpointConstraints = null;
            }

            public IReadOnlyList<IEndpointConstraint> EndpointConstraints { get; }

            public List<EndpointConstraintItem> Items { get; }
        }
    }

    public class EndpointConstraintItem
    {
        public EndpointConstraintItem(IEndpointConstraintMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            Metadata = metadata;
        }

        public IEndpointConstraint Constraint { get; set; }

        public IEndpointConstraintMetadata Metadata { get; }

        public bool IsReusable { get; set; }
    }

    public interface IEndpointConstraintProvider
    {
        int Order { get; }

        void OnProvidersExecuting(EndpointConstraintProviderContext context);

        void OnProvidersExecuted(EndpointConstraintProviderContext context);
    }

    public class EndpointConstraintProviderContext
    {
        public EndpointConstraintProviderContext(
            HttpContext context,
            Endpoint endpoint,
            IList<EndpointConstraintItem> items)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            HttpContext = context;
            Endpoint = endpoint;
            Results = items;
        }

        public HttpContext HttpContext { get; }

        public Endpoint Endpoint { get; }

        public IList<EndpointConstraintItem> Results { get; }
    }

    public class DefaultEndpointConstraintProvider : IEndpointConstraintProvider
    {
        /// <inheritdoc />
        public int Order => -1000;

        /// <inheritdoc />
        public void OnProvidersExecuting(EndpointConstraintProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            for (var i = 0; i < context.Results.Count; i++)
            {
                ProvideConstraint(context.Results[i], context.HttpContext.RequestServices);
            }
        }

        /// <inheritdoc />
        public void OnProvidersExecuted(EndpointConstraintProviderContext context)
        {
        }

        private void ProvideConstraint(EndpointConstraintItem item, IServiceProvider services)
        {
            // Don't overwrite anything that was done by a previous provider.
            if (item.Constraint != null)
            {
                return;
            }

            if (item.Metadata is IEndpointConstraint constraint)
            {
                item.Constraint = constraint;
                item.IsReusable = true;
                return;
            }

            if (item.Metadata is IEndpointConstraintFactory factory)
            {
                item.Constraint = factory.CreateInstance(services);
                item.IsReusable = factory.IsReusable;
                return;
            }
        }
    }

    public interface IEndpointConstraintFactory : IEndpointConstraintMetadata
    {
        bool IsReusable { get; }

        IEndpointConstraint CreateInstance(IServiceProvider services);
    }
}