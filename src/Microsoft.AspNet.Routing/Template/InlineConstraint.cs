﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Routing.Template
{
    public class InlineConstraint
    {
        public InlineConstraint([NotNull] string constraint)
        {
            Constraint = constraint;
        }

        public string Constraint { get; private set; }
    }
}