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
    /// A Nuwa theory test cases discoverer for Xunit.
    /// </summary>
    public class NuwaTheoryDiscoverer : TheoryDiscoverer
    {
        readonly IMessageSink diagnosticMessageSink;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuwaTheoryDiscoverer"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        public NuwaTheoryDiscoverer(IMessageSink diagnosticMessageSink)
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

            //If framework attribute is present, get additional test cases based on frames.
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
