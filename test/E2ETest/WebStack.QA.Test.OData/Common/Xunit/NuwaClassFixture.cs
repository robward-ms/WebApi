// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.Test.E2E.AspNet.OData.Common.Nuwa;
using Xunit;
using Xunit.Abstractions;

// The Nuwa framework requires that tests in each class are serialized due to the interaction between
// the NuwaClassFixture and the NuwaTestCase and the fact the Xunit creates a test class instance for each
// test but provides no direct access to the test class instance.
//
// Parallelism in the test framework is a feature that's new for (Xunit) version 2.
// Tests written in xUnit.net version 1 cannot be run in parallel against each other in the same assembly...
// By default, each test class is a unique test collection. Tests within the same test class will not run
// in parallel against each other.
//
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Microsoft.Test.E2E.AspNet.OData.Common.Xunit
{
    public class NuwaClassFixture : ITestCaseManager, IDisposable
    {
        private NuwaRunFrame runFrame;
        private ITypeInfo testClassInfo;
        private NuwaTestCase testCase;
        private bool classRegistered;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuwaClassFixture"/> class.
        /// </summary>
        public NuwaClassFixture()
        {
        }

        /// <summary>
        /// Dispose to this instance of the <see cref="NuwaClassFixture"/> class.
        /// </summary>
        public void Dispose()
        {
            // dispose all run frame
            if (runFrame != null)
            {
                try
                {
                    runFrame.Cleanup();
                }
                catch (Exception)
                {
                    // While failure may occur, don't let errors in tearing down the
                    // test fail the test.
                }
            }
        }

        /// <summary>
        /// Register an instance of the test class with the fixture.
        /// </summary>
        /// <param name="testClassInstance"></param>
        public void RegisterClass(NuwaTestBase testClassInstance)
        {
            if (this.testCase != null)
            {
                this.runFrame.Initialize(
                    this.testClassInfo.ToRuntimeType(),
                    testClassInstance,
                    this.testCase);
            }

            this.classRegistered = true;
            this.testCase = null;
        }

        /// <summary>
        /// Register an instance of the test class with the fixture.
        /// </summary>
        /// <param name="testMethod"></param>
        public bool RegisterTest(NuwaTestCase testCase)
        {
            // Class fixture creates frames, initialized them when first test registers.
            // Fixture passes test info and test class to initialize frame.
            if (this.runFrame == null)
            {
                this.testClassInfo = testCase.TestMethod.TestClass.Class;
                this.runFrame = new NuwaRunFrame(this.testClassInfo);
            }

            // Keep track of frame we should use for the next test, this is cached and used when
            // Initialize is called.
            this.classRegistered = false;
            this.testCase = testCase;
            return true;
        }

        /// <summary>
        /// Verify that a class instance registered with the fixture.
        /// </summary>
        public bool VerifyRegisterClass()
        {
            return this.classRegistered;
        }
    }
}
