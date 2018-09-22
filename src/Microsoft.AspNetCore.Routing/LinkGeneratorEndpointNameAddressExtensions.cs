﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using System;

namespace Microsoft.AspNetCore.Routing
{
    /// <summary>
    /// Extension methods for using <see cref="LinkGenerator"/> with and endpoint name.
    /// </summary>
    public static class LinkGeneratorEndpointNameAddressExtensions
    {
        private static readonly LinkGenerationTemplateOptions _templateOptions = new LinkGenerationTemplateOptions()
        {
            UseAmbientValues = false,
        };

        /// <summary>
        /// Generates a URI with an absolute path based on the provided values.
        /// </summary>
        /// <param name="generator">The <see cref="LinkGenerator"/>.</param>
        /// <param name="httpContext">The <see cref="HttpContext"/> associated with the current request.</param>
        /// <param name="endpointName">The endpoint name. Used to resolve endpoints.</param>
        /// <param name="values">The route values. Used to expand parameters in the route template. Optional.</param>
        /// <param name="pathBase">
        /// An optional URI path base. Prepended to the path in the resulting URI. If not provided, the value of <see cref="HttpRequest.PathBase"/> will be used.
        /// </param>
        /// <param name="fragment">An optional URI fragment. Appended to the resulting URI.</param>
        /// <param name="options">
        /// An optional <see cref="LinkOptions"/>. Settings on provided object override the settings with matching
        /// names from <c>RouteOptions</c>.
        /// </param>
        /// <returns>A URI with an absolute path, or <c>null</c>.</returns>
        public static string GetPathByName(
            this LinkGenerator generator,
            HttpContext httpContext,
            string endpointName,
            object values,
            PathString? pathBase = default,
            FragmentString fragment = default,
            LinkOptions options = default)
        {
            if (generator == null)
            {
                throw new ArgumentNullException(nameof(generator));
            }

            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (endpointName == null)
            {
                throw new ArgumentNullException(nameof(endpointName));
            }

            return generator.GetPathByAddress<string>(
                httpContext,
                endpointName,
                new RouteValueDictionary(values),
                ambientValues: null,
                pathBase,
                fragment,
                options);
        }

        /// <summary>
        /// Generates a URI with an absolute path based on the provided values.
        /// </summary>
        /// <param name="generator">The <see cref="LinkGenerator"/>.</param>
        /// <param name="endpointName">The endpoint name. Used to resolve endpoints.</param>
        /// <param name="values">The route values. Used to expand parameters in the route template. Optional.</param>
        /// <param name="pathBase">An optional URI path base. Prepended to the path in the resulting URI.</param>
        /// <param name="fragment">An optional URI fragment. Appended to the resulting URI.</param>
        /// <param name="options">
        /// An optional <see cref="LinkOptions"/>. Settings on provided object override the settings with matching
        /// names from <c>RouteOptions</c>.
        /// </param>
        /// <returns>A URI with an absolute path, or <c>null</c>.</returns>
        public static string GetPathByName(
            this LinkGenerator generator,
            string endpointName,
            object values,
            PathString pathBase = default,
            FragmentString fragment = default,
            LinkOptions options = default)
        {
            if (generator == null)
            {
                throw new ArgumentNullException(nameof(generator));
            }

            if (endpointName == null)
            {
                throw new ArgumentNullException(nameof(endpointName));
            }

            return generator.GetPathByAddress<string>(endpointName, new RouteValueDictionary(values), pathBase, fragment, options);
        }

        /// <summary>
        /// Generates an absolute URI based on the provided values.
        /// </summary>
        /// <param name="generator">The <see cref="LinkGenerator"/>.</param>
        /// <param name="httpContext">The <see cref="HttpContext"/> associated with the current request.</param>
        /// <param name="endpointName">The endpoint name. Used to resolve endpoints.</param>
        /// <param name="values">The route values. Used to expand parameters in the route template. Optional.</param>
        /// <param name="scheme">
        /// The URI scheme, applied to the resulting URI. Optional. If not provided, the value of <see cref="HttpRequest.Scheme"/> will be used.
        /// </param>
        /// <param name="host">
        /// The URI host/authority, applied to the resulting URI. Optional. If not provided, the value <see cref="HttpRequest.Host"/> will be used.
        /// </param>
        /// <param name="pathBase">
        /// An optional URI path base. Prepended to the path in the resulting URI. If not provided, the value of <see cref="HttpRequest.PathBase"/> will be used.
        /// </param>
        /// <param name="fragment">An optional URI fragment. Appended to the resulting URI.</param>
        /// <param name="options">
        /// An optional <see cref="LinkOptions"/>. Settings on provided object override the settings with matching
        /// names from <c>RouteOptions</c>.
        /// </param>
        /// <returns>A URI with an absolute path, or <c>null</c>.</returns>
        public static string GetUriByName(
            this LinkGenerator generator,
            HttpContext httpContext,
            string endpointName,
            object values,
            string scheme = default,
            HostString? host = default,
            PathString? pathBase = default,
            FragmentString fragment = default,
            LinkOptions options = default)
        {
            if (generator == null)
            {
                throw new ArgumentNullException(nameof(generator));
            }

            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (endpointName == null)
            {
                throw new ArgumentNullException(nameof(endpointName));
            }

            return generator.GetUriByAddress<string>(
                httpContext,
                endpointName,
                new RouteValueDictionary(values),
                ambientValues: null,
                scheme,
                host,
                pathBase,
                fragment,
                options);
        }

        /// <summary>
        /// Generates an absolute URI based on the provided values.
        /// </summary>
        /// <param name="generator">The <see cref="LinkGenerator"/>.</param>
        /// <param name="endpointName">The endpoint name. Used to resolve endpoints.</param>
        /// <param name="values">The route values. Used to expand parameters in the route template. Optional.</param>
        /// <param name="scheme">The URI scheme, applied to the resulting URI.</param>
        /// <param name="host">The URI host/authority, applied to the resulting URI.</param>
        /// <param name="pathBase">An optional URI path base. Prepended to the path in the resulting URI.</param>
        /// <param name="fragment">An optional URI fragment. Appended to the resulting URI.</param>
        /// <param name="options">
        /// An optional <see cref="LinkOptions"/>. Settings on provided object override the settings with matching
        /// names from <c>RouteOptions</c>.
        /// </param>
        /// <returns>An absolute URI, or <c>null</c>.</returns>
        public static string GetUriByName(
            this LinkGenerator generator,
            string endpointName,
            object values,
            string scheme,
            HostString host,
            PathString pathBase = default,
            FragmentString fragment = default,
            LinkOptions options = default)
        {
            if (generator == null)
            {
                throw new ArgumentNullException(nameof(generator));
            }

            if (endpointName == null)
            {
                throw new ArgumentNullException(nameof(endpointName));
            }

            if (string.IsNullOrEmpty(scheme))
            {
                throw new ArgumentException("A scheme must be provided.", nameof(scheme));
            }

            if (!host.HasValue)
            {
                throw new ArgumentException("A host must be provided.", nameof(host));
            }

            return generator.GetUriByAddress<string>(endpointName, new RouteValueDictionary(values), scheme, host, pathBase, fragment, options);
        }

        /// <summary>
        /// Gets a <see cref="LinkGenerationTemplate"/> based on the provided <paramref name="endpointName"/>.
        /// </summary>
        /// <param name="generator">The <see cref="LinkGenerator"/>.</param>
        /// <param name="endpointName">The endpoint name. Used to resolve endpoints. Optional.</param>
        /// <returns>
        /// A <see cref="LinkGenerationTemplate"/> if one or more endpoints matching the address can be found, otherwise <c>null</c>.
        /// </returns>
        public static LinkGenerationTemplate GetTemplateByName(this LinkGenerator generator, string endpointName)
        {
            if (generator == null)
            {
                throw new ArgumentNullException(nameof(generator));
            }

            if (endpointName == null)
            {
                throw new ArgumentNullException(nameof(endpointName));
            }

            return generator.GetTemplateByAddress<string>(endpointName, _templateOptions);
        }
    }
}
