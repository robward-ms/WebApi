using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Nuwa.Sdk
{
    /// <summary>
    /// the test command adopt specific host strategy
    /// </summary>
    public class NuwaTestCase : IXunitTestCase
    {
        private IXunitTestCase _innerTestCase;

        public NuwaTestCase(IXunitTestCase innerTestCase)
        {
            if (innerTestCase == null)
            {
                throw new ArgumentNullException("innerTestCase");
            }

            _innerTestCase = innerTestCase;
        }

        public RunFrame Frame { get; set; }

        public string DisplayName
        {
            get
            {
                return _innerTestCase.DisplayName;
            }
        }

        public IMethodInfo Method
        {
            get
            {
                return _innerTestCase.Method;
            }
        }

        public string SkipReason
        {
            get
            {
                return _innerTestCase.SkipReason;
            }
        }

        public ISourceInformation SourceInformation
        {
            get
            {
                return _innerTestCase.SourceInformation;
            }

            set
            {
                _innerTestCase.SourceInformation = value;
            }
        }

        public object[] TestMethodArguments
        {
            get
            {
                return _innerTestCase.TestMethodArguments;
            }
        }

        public Dictionary<string, List<string>> Traits
        {
            get
            {
                return _innerTestCase.Traits;
            }
        }

        public string UniqueID
        {
            get
            {
                return _innerTestCase.UniqueID;
            }
        }

        public ITestMethod TestMethod
        {
            get
            {
                return _innerTestCase.TestMethod;
            }
        }

        public void Deserialize(IXunitSerializationInfo info)
        {
            _innerTestCase.Deserialize(info);
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            _innerTestCase.Serialize(info);
        }

        public Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {

            Frame.Initialize(TestMethod.TestClass.Class.ToRuntimeType(), this);
            return _innerTestCase.RunAsync(diagnosticMessageSink, messageBus, constructorArguments, aggregator, cancellationTokenSource);
        }

    }
}