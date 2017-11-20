using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuwa.Sdk
{
    /// <summary>
    /// An interface used by <see cref="NuwaTestCase"/> to register with a test manager.
    /// </summary>
    public interface ITestCaseManager
    {
        /// <summary>
        /// Register an instance of the test class with the fixture.
        /// </summary>
        /// <param name="testMethod"></param>
        bool RegisterTest(NuwaTestCase testCase);

        /// <summary>
        /// Verify that a class instance registered with the fixture.
        /// </summary>
        bool VerifyRegisterClass();
    }
}
