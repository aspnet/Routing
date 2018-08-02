// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Routing
{
    public abstract class LinkGenerationTemplate
    {
        public string MakeUrl(object values)
        {
            return MakeUrl(values, options: null);
        }

        public abstract string MakeUrl(object values, LinkOptions options);
    }
}