// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.Test.E2E.AspNet.OData.Common.Nuwa
{
    /// <summary>
    /// An interface used by <see cref="NuwaTestCase"/> to register with a test manager.
    /// </summary>
    public interface ITestCaseManager
    {
        /// <summary>
        /// Register an instance of the test class with the fixture.
        /// </summary>
        /// <param name="testMethod"></param>
        bool RegisterTest(NuwaTestCase testCase);

        /// <summary>
        /// Verify that a class instance registered with the fixture.
        /// </summary>
        bool VerifyRegisterClass();
    }
}
