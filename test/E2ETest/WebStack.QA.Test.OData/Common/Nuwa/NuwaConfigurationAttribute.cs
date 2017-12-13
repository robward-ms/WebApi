// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Nuwa
{
    /// <summary>
    /// Mark the method which is used to adjust HttpConfiguration
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NuwaConfigurationAttribute : Attribute
    {
    }
}