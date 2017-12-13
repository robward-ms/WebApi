// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Test.E2E.AspNet.OData.Common.Nuwa;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Xunit
{
    /// <summary>
    /// A Nuwa fact test cases discoverer for Xunit.
    /// </summary>
    public class NuwaFactDiscoverer : FactDiscoverer
    {
        readonly IMessageSink diagnosticMessageSink;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuwaFactDiscoverer"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        public NuwaFactDiscoverer(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Needs to return test case.")]
        public override IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            // Use base class to do heavy-lifting of discovery.
            IEnumerable<IXunitTestCase> discoveredTestCases = base.Discover(discoveryOptions, testMethod, factAttribute);

            // If it's a valid Nuwa test case, wrap it in NuwaTestCase.
            ITypeInfo testClass = testMethod.TestClass.Class;
            foreach (var test in discoveredTestCases)
            {
                if ((!string.IsNullOrEmpty(test.SkipReason)) ||
                    (!NuwaTestCase.ValidateTypeUnderTest(testClass)))
                {
                    yield return test;
                }
                else
                {
                    yield return new NuwaTestCase(
                        diagnosticMessageSink,
                        discoveryOptions.MethodDisplayOrDefault(),
                        test.TestMethod,
                        test.TestMethodArguments);
                }
            }
        }
    }
}
