// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Xunit.Abstractions;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Nuwa
{
    /// <summary>
    /// RunFrame defines the environment in which a test command is run.
    /// 
    /// A RunFrame is created during test initialization, but
    /// it is not actually initialized until the first test command which requires
    /// this RunFrame is executed. The lazy pattern ensure resources are reserved
    /// when one test case is run individually.
    /// 
    /// Once a RunFrame is initialized, it will; be largely reused. But the Initialize
    /// still needs to be called so as to fill the value to the property in test class.
    /// RunFrame is disposed when the test is running its ClassFinished method. All
    /// elements are disposed by then.
    /// </summary>
    public class NuwaRunFrame
    {
        private bool _initialized;
        private KatanaSelfHostElement _hostElement;

        public NuwaRunFrame(ITypeInfo typeInfo)
        {
            _hostElement = new KatanaSelfHostElement(new TestTypeDescriptor(typeInfo));
            _initialized = false;
        }

        public void Initialize(Type testClassType, object testClassInstance, NuwaTestCase testCommand)
        {
            if (!_initialized)
            {
                _hostElement.Initialize();
                _initialized = true;
            }

            _hostElement.SetBaseAddress(testClassType, testClassInstance);
            ClientConfigurationElement.SetHttpclient(testClassType, testClassInstance, testCommand);
        }

        public void Cleanup()
        {
            _hostElement.Cleanup();
        }
    }
}