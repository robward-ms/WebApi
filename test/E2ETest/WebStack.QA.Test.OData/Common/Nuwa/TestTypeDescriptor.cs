// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit.Abstractions;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Nuwa
{
    /// <summary>
    /// TestDescriptor is a translator to convert test class type to useful information.
    /// </summary>
    public class TestTypeDescriptor
    {
        private ITypeInfo _testClassType;

        /// <summary>
        /// Ctor
        /// </summary>
        public TestTypeDescriptor(ITypeInfo testClassType)
        {
            _testClassType = testClassType;
        }

        /// <summary>
        /// The type information of the test class
        /// </summary>
        public ITypeInfo TestTypeInfo
        {
            get { return _testClassType; }
        }

        /// <summary>
        /// The method marked by <paramref name="NuwaConfigurationAttribute"/>
        /// </summary>
        public IMethodInfo ConfigureMethod
        {
            get
            {
                return GetDesignatedMethod<NuwaConfigurationAttribute>();
            }
        }

        /// <summary>
        /// The method marked by <paramref name="NuwaWebConfigAttribute"/>
        /// </summary>
        public IMethodInfo WebConfigMethod
        {
            get
            {
                return GetDesignatedMethod<NuwaWebConfigAttribute>();
            }
        }

        /// <summary>
        /// The assembly this test class belongs to
        /// </summary>
        public IAssemblyInfo TestAssembly
        {
            get
            {
                return this.TestTypeInfo.Assembly;
            }
        }

        /// <summary>
        /// Get the reflect method info based on it's attribute
        /// </summary>
        public IMethodInfo GetDesignatedMethod<T>() where T : Attribute
        {
            var markedMethod = this.TestTypeInfo.GetMethods(includePrivateMethods: true)
                .Where(m => m.GetCustomAttributes(typeof(T)).FirstOrDefault() != null);

            if (!markedMethod.Any())
            {
                return null;
            }
            else if (markedMethod.Count() == 1)
            {
                return markedMethod.First();
            }
            else
            {
                throw new InvalidOperationException(
                    "There are more than one methods are marked by attribute " + typeof(T).Name + ".");
            }
        }
    }
}