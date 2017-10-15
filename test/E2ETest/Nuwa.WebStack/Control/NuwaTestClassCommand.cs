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
    public class NuwaTestClassCommand : IDisposable
    {
        private Collection<RunFrame> _frames;
        private IRunFrameBuilder _frmBuilder;
        private bool disposedValue = false; // To detect redundant calls

        public NuwaTestClassCommand()
        {
            ValidateTypeUnderTest();

            var resolver = DependencyResolver.Instance;

            // autowiring
            _frmBuilder = resolver.Container.Resolve(
                typeof(IRunFrameBuilder),
                new NamedParameter("testClass", this))
                as IRunFrameBuilder;

            _frames = _frmBuilder.CreateFrames();

        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose all run frame
                    foreach (var rf in _frames)
                    {
                        try
                        {
                            rf.Cleanup();
                        }
                        catch (Exception)
                        {
                        }
                    }

                    disposedValue = true;
                }
            }
        }

        //    public override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod)
        //    {
        //        /// TODO - Advanced feature:
        //        /// 1. Frame filter, some cases can be filtered under some frame
        //        var combinations = from test in Proxy.EnumerateTestCommands(testMethod)
        //                           from frame in _frames
        //                           select new { TestCommand = test, RunFrame = frame };

        //        foreach (var each in combinations)
        //        {
        //            var isSkipped =
        //                (each.TestCommand is DelegatingTestCommand) ?
        //                (each.TestCommand as DelegatingTestCommand).InnerCommand is SkipCommand :
        //                (each.TestCommand is SkipCommand);

        //            if (isSkipped)
        //            {
        //                yield return each.TestCommand;
        //            }
        //            else
        //            {
        //                var testCommand = new NuwaTestCommand(each.TestCommand)
        //                {
        //                    Frame = each.RunFrame,
        //                    TestMethod = testMethod
        //                };

        //                yield return testCommand;
        //            }
        //        }
        //    }

        /// <summary>
        /// Validate the type under test before any actual operation is done. 
        /// 
        /// Exception will be thrown if the validation failed. The thrown exception
        /// is expected be caught in external frame.
        /// </summary>
        private void ValidateTypeUnderTest()
        {
            // check framework attribute
            if (NuwaTestClassCommand.GetNuwaFrameworkAttr(this) == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "The test class must be marked by {0}.",
                        typeof(NuwaFrameworkAttribute).Name));
            }

            // check configuration method attribute
            var configMethodAttr = TypeUnderTest.GetCustomAttributes<NuwaConfigurationAttribute>();
            if (configMethodAttr.Length > 1)
            {
                throw new InvalidOperationException(
                    string.Format("More than two methods are marked by {0}.", typeof(NuwaConfigurationAttribute).Name));
            }
        }
    }
}