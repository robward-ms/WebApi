// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Xunit
{
    /// <summary>
    /// Nuwa-specific Fact attribute used to attach a Nuwa discoverer.
    /// </summary>
    [XunitTestCaseDiscoverer("Microsoft.Test.E2E.AspNet.OData.Common.Xunit.NuwaFactDiscoverer", "Microsoft.Test.E2E.AspNet.OData")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class NuwaFactAttribute : FactAttribute
    {
    }
}