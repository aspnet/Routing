﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Routing
{
    /// <summary>
    /// Represents information about the route and virtual path that are the result of
    /// generating a URL with the ASP.NET routing middleware.
    /// </summary>
    public class VirtualPathData
    {
        private string _virtualPath;
        private readonly IDictionary<string, object> _dataToken;

        /// <summary>
        ///  Initializes a new instance of the <see cref="VirtualPathData"/> class.
        /// </summary>
        /// <param name="router">The object that is used to generate the URL.</param>
        /// <param name="virtualPath">The generated URL.</param>
        public VirtualPathData([NotNull] IRouter router, string virtualPath)
            : this(router, virtualPath, dataTokens: new RouteValueDictionary())
        {
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="VirtualPathData"/> class.
        /// </summary>
        /// <param name="router">The object that is used to generate the URL.</param>
        /// <param name="virtualPath">The generated URL.</param>
        /// <param name="dataTokens">The collection of custom values.</param>
        public VirtualPathData(
            [NotNull] IRouter router,
            string virtualPath,
            IDictionary<string, object> dataTokens)
        {
            Router = router;
            VirtualPath = virtualPath;

            _dataToken = new RouteValueDictionary();
            if (dataTokens != null)
            {
                foreach (var dataToken in dataTokens)
                {
                    _dataToken.Add(dataToken.Key, dataToken.Value);
                }
            }
        }

        /// <summary>
        /// Gets the collection of custom values for the <see cref="Router"/>.
        /// </summary>
        public IDictionary<string, object> DataTokens
        {
            get { return _dataToken; }
        }

        /// <summary>
        /// Gets or sets the <see cref="IRouter"/> that was used to generate the URL.
        /// </summary>
        public IRouter Router { get; set; }

        /// <summary>
        /// Gets or sets the URL that was generated from the <see cref="Router"/>.
        /// </summary>
        public string VirtualPath
        {
            get
            {
                return _virtualPath ?? string.Empty;
            }
            set
            {
                _virtualPath = value;
            }
        }
    }
}