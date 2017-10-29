using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nuwa.Control;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace WebStack.QA.Test.OData
{
    public class NuwaTestCaseDiscoverer : FactDiscoverer
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
            return testCases;

            // If framework attribute, get frames.
            //If frames but no fixture, fail test case or skip.
            //Otherwise, base class discover only.
            //Test case gets frame number, 1 to #frames, serialized it. -1 if no frame.
            //Class fixture creates frames, initialized them in constructor.
            //Test case puts frame id on fixture via constructor params before test.
            //Test case verifies class called fixture post test, otherwise fail.
            //Fixture is passed to test class constructor, otherwise test case fails when it can't find fixture and frame id >= 0.
            //Constructor calls to fixture with this.
            //Fixture passes test info and test class to initialize frame.
            //Fixture cleans up frames in dispose.

            //To get this to work:
            //Reference discoverer + nuwa test case
            //Add framework to class
            //Derived from base class, fixture
            //Has optional nuwa methods
            //Has required nuwa methods?
            //Constructor takes fixture, base class could need it to force constructor.
            //Constructor called fixture for setup


            // Return tests cases.
            //return NuwaTestClassCommand.EnumerateTestCommands(testMethod.TestClass.Class, testCases);
        }
    }
}
