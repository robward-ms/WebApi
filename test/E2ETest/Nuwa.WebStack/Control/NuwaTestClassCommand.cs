using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using Nuwa.DI;
using Nuwa.Sdk;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Nuwa.Control
{
    /// <summary>
    /// Define the class level execution of Nuwa
    /// </summary>
    public static class NuwaTestClassCommand
    {
        public static IAttributeInfo GetNuwaFrameworkAttr(ITypeInfo typeUnderTest)
        {
            return typeUnderTest.GetCustomAttributes<NuwaFrameworkAttribute>().FirstOrDefault();
        }

        /// <summary>
        /// Act before any test method is executed. All host strategies requested are set up in this method.
        /// </summary>
        /// <returns>Returns exception thrown during execution; null, otherwise.</returns>
        public static Collection<RunFrame> CreateFrames(ITypeInfo typeUnderTest)
        {
            if (ValidateTypeUnderTest(typeUnderTest))
            {
                // create run frames
                var resolver = DependencyResolver.Instance;

                // autowiring
                var frmBuilder = resolver.Container.Resolve(
                    typeof(IRunFrameBuilder),
                    new NamedParameter("testClass", typeUnderTest))
                    as IRunFrameBuilder;

                return frmBuilder.CreateFrames();
            }

            return new Collection<RunFrame>();
        }

        /// <summary>
        /// Act after all test methods are executed. All host strategies are released in this method. 
        /// </summary>
        /// <returns>Returns aggregated exception thrown during execution; null, otherwise.</returns>
        public static void ClassFinish(Collection<RunFrame> frames)
        {
            // dispose all run frame
            if (frames != null)
            {
                foreach (var rf in frames)
                {
                    try
                    {
                        rf.Cleanup();
                    }
                    catch (Exception)
                    {
                        // While failure may occur, don't let errors in tearing down the
                        // test fail the test.
                    }
                }
            }
        }

        public static IEnumerable<IXunitTestCase> EnumerateTestCommands(
            ITypeInfo typeUnderTest,
            IEnumerable<IXunitTestCase> discoveredTestCases,
            IMessageSink diagnosticMessageSink,
            TestMethodDisplay defaultMethodDisplay)
        {
            Collection<RunFrame> frames = CreateFrames(typeUnderTest);

            foreach (var test in discoveredTestCases)
            {
                if (!string.IsNullOrEmpty(test.SkipReason))
                {
                    yield return test;
                }
                else if (frames.Count == 0)
                {
                    yield return test;
                }
                else
                {
                    for (int i = 0; i < frames.Count; i++)
                    {
                        // Test case gets frame number, 0 to #frames -1. We really just need a placeholder for discovery,
                        // we'll attach to the actual frame during the test run.
                        yield return new NuwaTestCase(i, diagnosticMessageSink, defaultMethodDisplay, test.TestMethod, test.TestMethodArguments);
                    }
                }
            }
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
            if (NuwaTestClassCommand.GetNuwaFrameworkAttr(typeUnderTest) == null)
            {
                return false;
            }

            // check configuration method attribute
            var configMethodAttr = typeUnderTest.GetCustomAttributes<NuwaConfigurationAttribute>();
            if (configMethodAttr.Length > 1)
            {
                throw new InvalidOperationException(
                    string.Format("More than two methods are marked by {0}.", typeof(NuwaConfigurationAttribute).Name));
            }

            return true;
        }
    }
}