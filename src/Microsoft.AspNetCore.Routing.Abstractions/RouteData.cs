// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Routing
{
    /// <summary>
    /// Information about the current routing path.
    /// </summary>
    public class RouteData
    {
        private RouteValueDictionary _dataTokens;
        private IList<IRouter> _routers;
        private RouteValueDictionary _values;

        /// <summary>
        /// Creates a new <see cref="RouteData"/> instance.
        /// </summary>
        public RouteData()
        {
            // Perf: Avoid allocating collections unless needed.
        }

        /// <summary>
        /// Creates a new <see cref="RouteData"/> instance with values copied from <paramref name="other"/>.
        /// </summary>
        /// <param name="other">The other <see cref="RouteData"/> instance to copy.</param>
        public RouteData(RouteData other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            // Perf: Avoid allocating collections unless we need to make a copy.
            if (other._routers != null)
            {
                _routers = new List<IRouter>(other.Routers);
            }

            if (other._dataTokens != null)
            {
                _dataTokens = new RouteValueDictionary(other._dataTokens);
            }

            if (other._values != null)
            {
                _values = new RouteValueDictionary(other._values);
            }
        }

        /// <summary>
        /// Gets the data tokens produced by routes on the current routing path.
        /// </summary>
        public RouteValueDictionary DataTokens
        {
            get
            {
                if (_dataTokens == null)
                {
                    _dataTokens = new RouteValueDictionary();
                }

                return _dataTokens;
            }
        }

        /// <summary>
        /// Gets the list of <see cref="IRouter"/> instances on the current routing path.
        /// </summary>
        public IList<IRouter> Routers
        {
            get
            {
                if (_routers == null)
                {
                    _routers = new List<IRouter>();
                }

                return _routers;
            }
        }

        /// <summary>
        /// Gets the set of values produced by routes on the current routing path.
        /// </summary>
        public RouteValueDictionary Values
        {
            get
            {
                if (_values == null)
                {
                    _values = new RouteValueDictionary();
                }

                return _values;
            }
        }

        /// <summary>
        /// <para>
        /// Creates a snapshot of the current state of the <see cref="RouteData"/> before appending
        /// <paramref name="router"/> to <see cref="Routers"/>, merging <paramref name="values"/> into
        /// <see cref="Values"/>, and merging <paramref name="dataTokens"/> into <see cref="DataTokens"/>.
        /// </para>
        /// <para>
        /// Call <see cref="RouteDataSnapshot.Restore"/> to restore the state of this <see cref="RouteData"/>
        /// to the state at the time of calling
        /// <see cref="PushState(IRouter, RouteValueDictionary, RouteValueDictionary)"/>.
        /// </para>
        /// </summary>
        /// <param name="router">
        /// An <see cref="IRouter"/> to append to <see cref="Routers"/>. If <c>null</c>, then <see cref="Routers"/>
        /// will not be changed.
        /// </param>
        /// <param name="values">
        /// A <see cref="RouteValueDictionary"/> to merge into <see cref="Values"/>. If <c>null</c>, then
        /// <see cref="Values"/> will not be changed.
        /// </param>
        /// <param name="dataTokens">
        /// A <see cref="RouteValueDictionary"/> to merge into <see cref="DataTokens"/>. If <c>null</c>, then
        /// <see cref="DataTokens"/> will not be changed.
        /// </param>
        /// <returns>A <see cref="RouteDataSnapshot"/> that captures the current state.</returns>
        public RouteDataSnapshot PushState(IRouter router, RouteValueDictionary values, RouteValueDictionary dataTokens)
        {
            var snapshot = new RouteDataSnapshot(
                this,
                _dataTokens?.Count > 0 ? new RouteValueDictionary(_dataTokens) : null, 
                _routers?.Count > 0 ? new List<IRouter>(_routers) : null,
                _values?.Count > 0 ? new RouteValueDictionary(_values) : null);

            if (router != null)
            {
                Routers.Add(router);
            }

            if (values != null)
            {
                foreach (var kvp in values)
                {
                    if (kvp.Value != null)
                    {
                        Values[kvp.Key] = kvp.Value;
                    }
                }
            }

            if (dataTokens != null)
            {
                foreach (var kvp in dataTokens)
                {
                    DataTokens[kvp.Key] = kvp.Value;
                }
            }

            return snapshot;
        }

        /// <summary>
        /// A snapshot of the state of a <see cref="RouteData"/> instance.
        /// </summary>
        public struct RouteDataSnapshot
        {
            private readonly RouteData _routeData;
            private readonly RouteValueDictionary _dataTokens;
            private readonly IList<IRouter> _routers;
            private readonly RouteValueDictionary _values;

            /// <summary>
            /// Creates a new <see cref="RouteDataSnapshot"/> for <paramref name="routeData"/>.
            /// </summary>
            /// <param name="routeData">The <see cref="RouteData"/>.</param>
            /// <param name="dataTokens">The data tokens.</param>
            /// <param name="routers">The routers.</param>
            /// <param name="values">The route values.</param>
            public RouteDataSnapshot(
                RouteData routeData,
                RouteValueDictionary dataTokens,
                IList<IRouter> routers,
                RouteValueDictionary values)
            {
                if (routeData == null)
                {
                    throw new ArgumentNullException(nameof(routeData));
                }

                _routeData = routeData;
                _dataTokens = dataTokens;
                _routers = routers;
                _values = values;
            }

            /// <summary>
            /// Restores the <see cref="RouteData"/> to the captured state.
            /// </summary>
            public void Restore()
            {
                if (_routeData._dataTokens == null && _dataTokens == null)
                {
                    // Do nothing
                }
                else if (_dataTokens == null)
                {
                    _routeData._dataTokens.Clear();
                }
                else
                {
                    _routeData._dataTokens.Clear();

                    foreach (var kvp in _dataTokens)
                    {
                        _routeData._dataTokens.Add(kvp.Key, kvp.Value);
                    }
                }

                if (_routeData._routers == null && _routers == null)
                {
                    // Do nothing
                }
                else if (_routers == null)
                {
                    _routeData._routers.Clear();
                }
                else
                {
                    _routeData._routers.Clear();

                    for (var i = 0; i < _routers.Count; i++)
                    {
                        _routeData._routers.Add(_routers[i]);
                    }
                }

                if (_routeData._values == null && _values == null)
                {
                    // Do nothing
                }
                else if (_values == null)
                {
                    _routeData._values.Clear();
                }
                else
                {
                    _routeData._values.Clear();

                    foreach (var kvp in _values)
                    {
                        _routeData._values.Add(kvp.Key, kvp.Value);
                    }
                }
            }
        }
    }
}