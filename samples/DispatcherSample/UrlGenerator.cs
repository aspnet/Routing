﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Dispatcher;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DispatcherSample
{
    public static class UrlGenerator
    {
        //Find match from values to a template
        public static string GenerateURL(RouteValueDictionary routeValues, HttpContext context)
        {
            var address = FindAddress(routeValues);
            return $"RouteName: {address.DisplayName} URL: /{address.RouteValueDictionary["Character"]}/{address.RouteValueDictionary["Movie"]}";
        }

        //Look up the Addresses table
        private static RouteValueAddress FindAddress(RouteValueDictionary routeValues)
        {
            var addressTable = new RouteValueAddressTable();
            var addressMatch = new RouteValueAddress(null, new RouteValueDictionary());
            foreach (var address in addressTable.Addresses)
            {
                foreach (var key in address.RouteValueDictionary.Keys)
                {
                    if (!routeValues.Keys.Contains(key))
                    {
                        addressMatch.RouteValueDictionary.Clear();
                        break;
                    }

                    if (routeValues.Values.Contains(address.RouteValueDictionary[key]))
                    {
                        addressMatch.RouteValueDictionary[key] = routeValues[key];
                    }
                }

                if (addressMatch.RouteValueDictionary.Count == routeValues.Count)
                {
                    return new RouteValueAddress(address.DisplayName, address.RouteValueDictionary);
                }
                else
                {
                    addressMatch.RouteValueDictionary.Clear();
                }
            }

            return addressMatch;
        }
    }
}
