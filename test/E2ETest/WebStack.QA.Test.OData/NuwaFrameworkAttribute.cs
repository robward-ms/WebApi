// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Reflection;
using Xunit.Sdk;

namespace WebStack.QA.Test.OData
{
    /// <summary>
    /// NuwaFrameworkAttribute is used to mark a Nuwa test class. It's derived from an Xunit
    /// BeforeAfterTestAttribute to allow injection of code before and after each test.
    /// This class leverages NuwaFixture and fixtures must reside in the same assembly as
    /// the tests that use them.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NuwaFrameworkAttribute : BeforeAfterTestAttribute
    {
        public NuwaFrameworkAttribute()
        {
        }

        /// <summary>
        /// Restores the original <see cref="Thread.CurrentPrincipal"/>.
        /// </summary>
        /// <param name="methodUnderTest">The method under test</param>
        public override void After(MethodInfo methodUnderTest)
        {
            //methodUnderTest.DeclaringType;
        }

        /// <summary>
        /// Stores the current <see cref="Thread.CurrentPrincipal"/> and replaces it with
        /// a new role identified in constructor.
        /// </summary>
        /// <param name="methodUnderTest">The method under test</param>
        public override void Before(MethodInfo methodUnderTest)
        {
            //methodUnderTest.DeclaringType;
        }
    }
}

