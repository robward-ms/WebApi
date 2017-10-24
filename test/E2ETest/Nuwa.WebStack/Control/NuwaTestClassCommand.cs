using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using Nuwa.DI;
using Nuwa.Sdk;
using Xunit.Abstractions;

namespace Nuwa.Control
{
    /// <summary>
    /// Define the class level execution of Nuwa
    /// </summary>
    public class NuwaTestClassCommand
    {
        private Collection<RunFrame> _frames;

        public NuwaTestClassCommand()
        {
        }

        public static IAttributeInfo GetNuwaFrameworkAttr(ITypeInfo typeUnderTest)
        {
            return typeUnderTest.GetCustomAttributes<NuwaFrameworkAttribute>().FirstOrDefault();
        }

        /// <summary>
        /// Act before any test method is executed. All host strategies requested are set up in this method.
        /// </summary>
        /// <returns>Returns exception thrown during execution; null, otherwise.</returns>
        public void ClassStart(ITypeInfo typeUnderTest)
        {
            ValidateTypeUnderTest(typeUnderTest);

            // create run frames
            var resolver = DependencyResolver.Instance;

            // autowiring
            var frmBuilder = resolver.Container.Resolve(
                typeof(IRunFrameBuilder),
                new NamedParameter("testClass", this))
                as IRunFrameBuilder;

            _frames = frmBuilder.CreateFrames();
        }

        /// <summary>
        /// Act after all test methods are executed. All host strategies are released in this method. 
        /// </summary>
        /// <returns>Returns aggregated exception thrown during execution; null, otherwise.</returns>
        public void ClassFinish(ITypeInfo typeUnderTest)
        {
            var exceptions = new List<Exception>();

            // dispose all run frame
            foreach (var rf in _frames)
            {
                try
                {
                    rf.Cleanup();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count != 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        public IEnumerable<ITestCase> EnumerateTestCommands(ITestMethod testMethod)
        {
            ///// TODO - Advanced feature:
            ///// 1. Frame filter, some cases can be filtered under some frame
            //var combinations = from test in Proxy.EnumerateTestCommands(testMethod)
            //                   from frame in _frames
            //                   select new { TestCommand = test, RunFrame = frame };

            //foreach (var each in combinations)
            //{
            //    var isSkipped =
            //        (each.TestCommand is DelegatingTestCommand) ?
            //        (each.TestCommand as DelegatingTestCommand).InnerCommand is SkipCommand :
            //        (each.TestCommand is SkipCommand);

            //    if (isSkipped)
            //    {
            //        yield return each.TestCommand;
            //    }
            //    else
            //    {
            //        var testCommand = new NuwaTestCase(each.TestCommand)
            //        {
            //            Frame = each.RunFrame,
            //            TestMethod = testMethod
            //        };

            //        yield return testCommand;
            //    }
            //}
        }

        /// <summary>
        /// Validate the type under test before any actual operation is done. 
        /// 
        /// Exception will be thrown if the validation failed. The thrown exception
        /// is expected be caught in external frame.
        /// </summary>
        private void ValidateTypeUnderTest(ITypeInfo typeUnderTest)
        {
            // check framework attribute
            if (NuwaTestClassCommand.GetNuwaFrameworkAttr(typeUnderTest) == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "The test class must be marked by {0}.",
                        typeof(NuwaFrameworkAttribute).Name));
            }

            // check configuration method attribute
            var configMethodAttr = typeUnderTest.GetCustomAttributes<NuwaConfigurationAttribute>();
            if (configMethodAttr.Length > 1)
            {
                throw new InvalidOperationException(
                    string.Format("More than two methods are marked by {0}.", typeof(NuwaConfigurationAttribute).Name));
            }
        }
    }
}