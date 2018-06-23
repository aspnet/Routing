// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BenchmarkDotNet.Attributes;

namespace Microsoft.AspNetCore.Routing
{
    public class RouteValueDictionaryBenchmark
    {
        [Benchmark]
        public RouteValueDictionary AddSingleItem()
        {
            var dictionary = new RouteValueDictionary();
            dictionary.Add("action", "Index");
            return dictionary;
        }

        [Benchmark]
        public RouteValueDictionary AddThreeItems()
        {
            var dictionary = new RouteValueDictionary();
            dictionary.Add("action", "Index");
            dictionary.Add("controller", "Home");
            dictionary.Add("id", "15");
            return dictionary;
        }

        [Benchmark]
        public RouteValueDictionary SetSingleItem()
        {
            var dictionary = new RouteValueDictionary();
            dictionary.Add("action", "Index");
            return dictionary;
        }

        [Benchmark]
        public RouteValueDictionary SetThreeItems()
        {
            var dictionary = new RouteValueDictionary();
            dictionary.Add("action", "Index");
            dictionary.Add("controller", "Home");
            dictionary.Add("id", "15");
            return dictionary;
        }
    }
}
