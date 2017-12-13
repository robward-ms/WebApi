// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.Test.E2E.AspNet.OData.Common.Nuwa;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Xunit
{
    /// <summary>
    /// The NuwaTestBaseClass is used to attach a class fixture to Nuwa-based tests and to force
    /// a constructor to take in the fixture.
    /// </summary>
    [NuwaFramework]
    public class NuwaTestBase : IClassFixture<NuwaClassFixture>
    {
        private string baseAddress = null;

        public NuwaTestBase(NuwaClassFixture fixture)
        {
            // Pass this test class instance to the fixture so it
            // can set properties on us.
            fixture.RegisterClass(this);
        }

        [NuwaBaseAddress]
        public string BaseAddress
        {
            get
            {
                return baseAddress;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.baseAddress = value.Replace("localhost", Environment.MachineName);
                }
            }
        }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }
    }
}
