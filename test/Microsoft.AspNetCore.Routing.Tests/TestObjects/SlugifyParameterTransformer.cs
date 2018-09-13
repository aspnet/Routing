﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.Routing.TestObjects
{
    public class SlugifyParameterTransformer : IParameterTransformer
    {
        public string Transform(string value)
        {
            // Slugify value
            return Regex.Replace(value, "([a-z])([A-Z])", "$1-$2").ToLower();
        }
    }
}
