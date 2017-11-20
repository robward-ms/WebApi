// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Xunit;
using Xunit.Sdk;

namespace WebStack.QA.Test.OData
{
    /// <summary>
    /// Nuwa-specific Theory attribute used to attach a Nuwa discoverer.
    /// </summary>
    [XunitTestCaseDiscoverer("WebStack.QA.Test.OData.NuwaTheoryDiscoverer", "WebStack.QA.Test.OData")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class NuwaTheoryAttribute : TheoryAttribute
    {
    }
}