using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nuwa.Control;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace WebStack.QA.Test.OData
{
    public class NuwaTestCaseDiscoverer : TheoryDiscoverer
    {
        readonly IMessageSink diagnosticMessageSink;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuwaTestCaseDiscoverer"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        public NuwaTestCaseDiscoverer(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Needs to return test case.")]
        public override IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            // Connect to NuwaTestClassCommand
            IEnumerable<IXunitTestCase> testCases = base.Discover(discoveryOptions, testMethod, factAttribute);

            // Return tests cases.
            return NuwaTestClassCommand.EnumerateTestCommands(testMethod.TestClass.Class, testCases);
        }
    }
}
