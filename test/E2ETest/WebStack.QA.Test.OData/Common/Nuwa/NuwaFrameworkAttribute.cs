// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Nuwa
{
    /// <summary>
    /// NuwaFrameworkAttribute is used to mark a Nuwa test class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NuwaFrameworkAttribute : Attribute
    {
        public NuwaFrameworkAttribute()
        {
        }
    }
}
