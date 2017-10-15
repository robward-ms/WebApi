// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Xunit;

namespace WebStack.QA.Test.OData
{
    /// <summary>
    /// This is a Nuwa test base class designed to associate the NuwaFixture with test classes
    /// To inject class-wide configuration. This class leverages NuwaFixture and
    /// fixtures must reside in the same assembly as the tests that use them.
    /// </summary>
    class NuwaTestClass : IClassFixture<NuwaFixture>
    {
        NuwaFixture fixture;

        public NuwaTestClass(NuwaFixture fixture)
        {
            this.fixture = fixture;
        }
    }
}
