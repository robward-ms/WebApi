using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Nuwa.Sdk
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
            int frameId,
            IMessageSink diagnosticMessageSink,
            TestMethodDisplay defaultMethodDisplay,
            ITestMethod testMethod,
            object[] testMethodArguments = null)
            : base(diagnosticMessageSink, defaultMethodDisplay, testMethod, testMethodArguments)
        {
            FrameId = frameId;
        }

        public int FrameId { get; private set; }

        protected override string GetDisplayName(IAttributeInfo factAttribute, string displayName)
        {
            string newDisplayName = base.GetDisplayName(factAttribute, displayName);
            return newDisplayName + " Frame " + FrameId.ToString();
        }

        public override void Serialize(IXunitSerializationInfo data)
        {
            base.Serialize(data);
            data.AddValue("FrameId", FrameId);
        }

        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);
            FrameId = data.GetValue<int>("FrameId");
        }

        protected override string GetUniqueID()
        {
            string id = base.GetUniqueID();
            return id + " -" + FrameId.ToString();
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
    }
}