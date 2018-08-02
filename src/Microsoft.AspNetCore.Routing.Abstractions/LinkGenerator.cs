// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing
{
    /// <summary>
    /// Defines a contract to generate urls to endpoints.
    /// </summary>
    public abstract class LinkGenerator
    {
        /// <summary>
        /// Generates a URL with an absolute path.
        /// </summary>
        /// <param name="values">An object that contains route values.</param>
        /// <returns>The generated absolute URL.</returns>
        public string GetLink(object values)
        {
            return GetLink(httpContext: null, routeName: null, values, options: null);
        }

        /// <summary>
        /// Generates a URL with an absolute path.
        /// </summary>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="options">The <see cref="LinkOptions"/>.</param>
        /// <returns>The generated absolute URL.</returns>
        public string GetLink(object values, LinkOptions options)
        {
            return GetLink(httpContext: null, routeName: null, values, options);
        }

        /// <summary>
        /// Tries generating a url with an absolute path.
        /// </summary>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="link">The generated absolute URL.</param>
        /// <returns><c>true</c> if a url was able to be generated successfully, <c>false</c> otherwise.</returns>
        public bool TryGetLink(object values, out string link)
        {
            return TryGetLink(httpContext: null, routeName: null, values, options: null, out link);
        }

        /// <summary>
        /// Tries generating a url with an absolute path.
        /// </summary>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="options">The <see cref="LinkOptions"/>.</param>
        /// <param name="link">The generated absolute URL.</param>
        /// <returns><c>true</c> if a url was able to be generated successfully, <c>false</c> otherwise.</returns>
        public bool TryGetLink(object values, LinkOptions options, out string link)
        {
            return TryGetLink(httpContext: null, routeName: null, values, options, out link);
        }

        /// <summary>
        /// Generates a URL with an absolute path.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> associated with current request.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <returns>The generated absolute URL.</returns>
        public string GetLink(HttpContext httpContext, object values)
        {
            return GetLink(httpContext, routeName: null, values, options: null);
        }

        /// <summary>
        /// Tries generating a url with an absolute path.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> associated with current request.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="link">The generated absolute URL.</param>
        /// <returns><c>true</c> if a url was able to be generated successfully, <c>false</c> otherwise.</returns>
        public bool TryGetLink(HttpContext httpContext, object values, out string link)
        {
            return TryGetLink(httpContext, routeName: null, values, options: null, out link);
        }

        /// <summary>
        /// Generates a URL with an absolute path.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> associated with current request.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="options">The <see cref="LinkOptions"/>.</param>
        /// <returns>The generated absolute URL.</returns>
        public string GetLink(HttpContext httpContext, object values, LinkOptions options)
        {
            return GetLink(httpContext, routeName: null, values, options);
        }

        /// <summary>
        /// Tries generating a url with an absolute path.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> associated with current request.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="options">The <see cref="LinkOptions"/>.</param>
        /// <param name="link">The generated absolute URL.</param>
        /// <returns><c>true</c> if a url was able to be generated successfully, <c>false</c> otherwise.</returns>
        public bool TryGetLink(HttpContext httpContext, object values, LinkOptions options, out string link)
        {
            return TryGetLink(httpContext, routeName: null, values, options, out link);
        }

        /// <summary>
        /// Generates a URL with an absolute path.
        /// </summary>
        /// <param name="routeName">The name of the route to generate the url to.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <returns>The generated absolute URL.</returns>
        public string GetLink(string routeName, object values)
        {
            return GetLink(httpContext: null, routeName, values, options: null);
        }

        /// <summary>
        /// Tries generating a url with an absolute path.
        /// </summary>
        /// <param name="routeName">The name of the route to generate the url to.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="link">The generated absolute URL.</param>
        /// <returns><c>true</c> if a url was able to be generated successfully, <c>false</c> otherwise.</returns>
        public bool TryGetLink(string routeName, object values, out string link)
        {
            return TryGetLink(httpContext: null, routeName, values, options: null, out link);
        }

        /// <summary>
        /// Generates a URL with an absolute path.
        /// </summary>
        /// <param name="routeName">The name of the route to generate the url to.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="options">The <see cref="LinkOptions"/>.</param>
        /// <returns>The generated absolute URL.</returns>
        public string GetLink(string routeName, object values, LinkOptions options)
        {
            return GetLink(httpContext: null, routeName, values, options);
        }

        /// <summary>
        /// Tries generating a url with an absolute path.
        /// </summary>
        /// <param name="routeName">The name of the route to generate the url to.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="options">The <see cref="LinkOptions"/>.</param>
        /// <param name="link">The generated absolute URL.</param>
        /// <returns><c>true</c> if a url was able to be generated successfully, <c>false</c> otherwise.</returns>
        public bool TryGetLink(string routeName, object values, LinkOptions options, out string link)
        {
            return TryGetLink(httpContext: null, routeName, values, options, out link);
        }

        /// <summary>
        /// Generates a URL with an absolute path.
        /// </summary>
        /// <param name="routeName">The name of the route to generate the url to.</param>
        /// <param name="httpContext">The <see cref="HttpContext"/> associated with current request.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <returns>The generated absolute URL.</returns>
        public string GetLink(HttpContext httpContext, string routeName, object values)
        {
            return GetLink(httpContext, routeName, values, options: null);
        }

        /// <summary>
        /// Tries generating a url with an absolute path.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> associated with current request.</param>
        /// <param name="routeName">The name of the route to generate the url to.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="link">The generated absolute URL.</param>
        /// <returns><c>true</c> if a url was able to be generated successfully, <c>false</c> otherwise.</returns>
        public bool TryGetLink(HttpContext httpContext, string routeName, object values, out string link)
        {
            return TryGetLink(httpContext, routeName, values, options: null, out link);
        }

        /// <summary>
        /// Generates a URL with an absolute path.
        /// </summary>
        /// <param name="routeName">The name of the route to generate the url to.</param>
        /// <param name="httpContext">The <see cref="HttpContext"/> associated with current request.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="options">The <see cref="LinkOptions"/>.</param>
        /// <returns>The generated absolute URL.</returns>
        public string GetLink(HttpContext httpContext, string routeName, object values, LinkOptions options)
        {
            if (TryGetLink(httpContext, routeName, values, options, out var link))
            {
                return link;
            }

            throw new InvalidOperationException("Could not find a matching endpoint to generate a link.");
        }

        /// <summary>
        /// Tries generating a url with an absolute path.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/> associated with current request.</param>
        /// <param name="routeName">The name of the route to generate the url to.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="options">The <see cref="LinkOptions"/>.</param>
        /// <param name="link">The generated absolute URL.</param>
        /// <returns><c>true</c> if a url was able to be generated successfully, <c>false</c> otherwise.</returns>
        public abstract bool TryGetLink(
            HttpContext httpContext,
            string routeName,
            object values,
            LinkOptions options,
            out string link);

        /// <summary>
        /// Generates a URL with an absolute path.
        /// </summary>
        /// <param name="address">The information used to look up for endpoints to generate a url.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <returns>The generated absolute URL.</returns>
        public string GetLinkByAddress<TAddress>(TAddress address, object values)
        {
            return GetLinkByAddress(address, httpContext: null, values, options: null);
        }

        /// <summary>
        /// Tries generating a url with an absolute path.
        /// </summary>
        /// <param name="address">The information used to look up for endpoints to generate a url.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="link">The generated absolute URL.</param>
        /// <returns><c>true</c> if a url was able to be generated successfully, <c>false</c> otherwise.</returns>
        public bool TryGetLinkByAddress<TAddress>(TAddress address, object values, out string link)
        {
            return TryGetLinkByAddress(address, values, options: null, out link);
        }

        /// <summary>
        /// Generates a URL with an absolute path.
        /// </summary>
        /// <param name="address">The information used to look up for endpoints to generate a url.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="options">The <see cref="LinkOptions"/>.</param>
        /// <returns>The generated absolute URL.</returns>
        public string GetLinkByAddress<TAddress>(TAddress address, object values, LinkOptions options)
        {
            return GetLinkByAddress(address, httpContext: null, values, options);
        }

        /// <summary>
        /// Tries generating a url with an absolute path.
        /// </summary>
        /// <param name="address">The information used to look up for endpoints to generate a url.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="options">The <see cref="LinkOptions"/>.</param>
        /// <param name="link">The generated absolute URL.</param>
        /// <returns><c>true</c> if a url was able to be generated successfully, <c>false</c> otherwise.</returns>
        public bool TryGetLinkByAddress<TAddress>(
            TAddress address,
            object values,
            LinkOptions options,
            out string link)
        {
            return TryGetLinkByAddress(address, httpContext: null, values, options, out link);
        }

        /// <summary>
        /// Generates a URL with an absolute path.
        /// </summary>
        /// <param name="address">The information used to look up for endpoints to generate a url.</param>
        /// <param name="httpContext">The <see cref="HttpContext"/> associated with current request.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <returns>The generated absolute URL.</returns>
        public string GetLinkByAddress<TAddress>(TAddress address, HttpContext httpContext, object values)
        {
            return GetLinkByAddress(address, httpContext, values, options: null);
        }

        /// <summary>
        /// Tries generating a url with an absolute path.
        /// </summary>
        /// <param name="address">The information used to look up for endpoints to generate a url.</param>
        /// <param name="httpContext">The <see cref="HttpContext"/> associated with current request.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="link">The generated absolute URL.</param>
        /// <returns><c>true</c> if a url was able to be generated successfully, <c>false</c> otherwise.</returns>
        public bool TryGetLinkByAddress<TAddress>(
            TAddress address,
            HttpContext httpContext,
            object values,
            out string link)
        {
            return TryGetLinkByAddress(address, httpContext, values, options: null, out link);
        }

        /// <summary>
        /// Generates a URL with an absolute path.
        /// </summary>
        /// <param name="address">The information used to look up for endpoints to generate a url.</param>
        /// <param name="httpContext">The <see cref="HttpContext"/> associated with current request.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="options">The <see cref="LinkOptions"/>.</param>
        /// <returns>The generated absolute URL.</returns>
        public string GetLinkByAddress<TAddress>(
            TAddress address,
            HttpContext httpContext,
            object values,
            LinkOptions options)
        {
            if (TryGetLinkByAddress(address, httpContext, values, options, out var link))
            {
                return link;
            }

            throw new InvalidOperationException("Could not find a matching endpoint to generate a link.");
        }

        /// <summary>
        /// Tries generating a url with an absolute path.
        /// </summary>
        /// <param name="address">The information used to look up for endpoints to generate a url.</param>
        /// <param name="httpContext">The <see cref="HttpContext"/> associated with current request.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="options">The <see cref="LinkOptions"/>.</param>
        /// <param name="link">The generated absolute URL.</param>
        /// <returns><c>true</c> if a url was able to be generated successfully, <c>false</c> otherwise.</returns>
        public abstract bool TryGetLinkByAddress<TAddress>(
            TAddress address,
            HttpContext httpContext,
            object values,
            LinkOptions options,
            out string link);

        public LinkGenerationTemplate GetTemplate(object values)
        {
            return GetTemplate(httpContext: null, routeName: null, values);
        }

        public LinkGenerationTemplate GetTemplate(string routeName, object values)
        {
            return GetTemplate(httpContext: null, routeName, values);
        }

        public LinkGenerationTemplate GetTemplate(HttpContext httpContext, object values)
        {
            return GetTemplate(httpContext, routeName: null, values);
        }

        public abstract LinkGenerationTemplate GetTemplate(HttpContext httpContext, string routeName, object values);

        public LinkGenerationTemplate GetTemplateByAddress<TAddress>(TAddress address)
        {
            return GetTemplateByAddress(address, httpContext: null);
        }

        public abstract LinkGenerationTemplate GetTemplateByAddress<TAddress>(
            TAddress address,
            HttpContext httpContext);
    }
}
