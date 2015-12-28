﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Routing.Internal
{
    public struct BufferValue
    {
        public BufferValue(string value, bool requiresEncoding)
        {
            Value = value;
            RequiresEncoding = requiresEncoding;
        }

        public bool RequiresEncoding { get; }

        public string Value { get; }
    }
}
