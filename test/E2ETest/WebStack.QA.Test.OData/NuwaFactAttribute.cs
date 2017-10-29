// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Xunit.Sdk;

namespace Xunit
{
    [XunitTestCaseDiscoverer("WebStack.QA.Test.OData.NuwaTestCaseDiscoverer", "WebStack.QA.Test.OData")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class NuwaFactAttribute : FactAttribute
    {
    }
}