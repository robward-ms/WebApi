// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Nuwa
{
    /// <summary>
    /// the test command adopt specific host strategy
    /// </summary>
    public class NuwaTestCase : XunitTestCase
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer", true)]
        public NuwaTestCase() { }

        public NuwaTestCase(
            IMessageSink diagnosticMessageSink,
            TestMethodDisplay defaultMethodDisplay,
            ITestMethod testMethod,
            object[] testMethodArguments = null)
            : base(diagnosticMessageSink, defaultMethodDisplay, testMethod, testMethodArguments)
        {
        }

        public override async Task<RunSummary> RunAsync(
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            // Find fixture and register.
            ITestCaseManager manager = null;
            foreach (object arg in constructorArguments)
            {
                manager = arg as ITestCaseManager;
                if (manager != null)
                {
                    manager.RegisterTest(this);
                    break;
                }
            }

            // If fixture not found, fail this test case.
            RunSummary runSummary = new RunSummary() { Total = 1 };
            if (manager == null)
            {
                runSummary.Failed++;
                string output = "Class fixture not found. Test class must implement constructor accepting NuwaClassFixture";
                TestFailed testResult = new TestFailed(new XunitTest(this, this.DisplayName), 0, output, new Exception(output));
                messageBus.QueueMessage(testResult);
            }
            else
            {
                // Run the test case.
                runSummary = await base.RunAsync(diagnosticMessageSink, messageBus, constructorArguments, aggregator, cancellationTokenSource);

                // Verify class registered
                if (((runSummary.Failed + runSummary.Skipped) == 0) && (!manager.VerifyRegisterClass()))
                {
                    runSummary.Failed++;
                    string output = "Class fixture not called. Test class must implement constructor calling NuwaClassFixture.RegisterClass()";
                    TestFailed testResult = new TestFailed(new XunitTest(this, this.DisplayName), 0, output, new Exception(output));
                    messageBus.QueueMessage(testResult);
                }
            }

            return runSummary;
        }

        /// <summary>
        /// Validate the type under test before any actual operation is done. 
        /// 
        /// Exception will be thrown if the validation failed. The thrown exception
        /// is expected be caught in external frame.
        /// </summary>
        public static bool ValidateTypeUnderTest(ITypeInfo typeUnderTest)
        {
            // check framework attribute
            IAttributeInfo frameworkAttr = typeUnderTest.GetCustomAttributes(typeof(NuwaFrameworkAttribute)).FirstOrDefault();
            if (frameworkAttr == null)
            {
                return false;
            }

            // check configuration method attribute
            var configMethodAttr = typeUnderTest.GetCustomAttributes(typeof(NuwaConfigurationAttribute));
            if (configMethodAttr.Count() > 1)
            {
                throw new InvalidOperationException(
                    string.Format("More than two methods are marked by {0}.", typeof(NuwaConfigurationAttribute).Name));
            }

            return true;
        }
    }
}