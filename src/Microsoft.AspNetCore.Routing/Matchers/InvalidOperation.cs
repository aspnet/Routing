// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.Serialization;

namespace Microsoft.AspNetCore.Routing.Matchers
{
    [Serializable]
    internal class InvalidOperation : Exception
    {
        public InvalidOperation()
        {
        }

        public InvalidOperation(string message) : base(message)
        {
        }

        public InvalidOperation(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidOperation(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}