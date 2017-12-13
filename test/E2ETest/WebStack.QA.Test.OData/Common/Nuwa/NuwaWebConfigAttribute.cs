// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Nuwa
{
    /// <summary>
    /// Mark the method user uses to update web.config in web-host scenario.
    /// 
    /// The marked method is expected to be a static method accept a WebConfigHelper
    /// instance and return void. If the test is running under host strategy
    /// other than WebHost, the marked method will not be invoked.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class NuwaWebConfigAttribute : Attribute
    {
        public NuwaWebConfigAttribute()
        {
        }
    }
}