// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nuwa.Control;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace WebStack.QA.Test.OData
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
            IEnumerable<IXunitTestCase> testCases = base.Discover(discoveryOptions, testMethod, factAttribute);

            //If framework attribute, get frames.
            ITypeInfo testClass = testMethod.TestClass.Class;
            if (NuwaTestClassCommand.ValidateTypeUnderTest(testClass))
            {
                // Add test cases for each frame.
                testCases = NuwaTestClassCommand.EnumerateTestCommands(testClass, testCases, diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault());
            }

            // Return tests cases.
            return testCases;
        }
    }
}
