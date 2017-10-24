using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nuwa.Sdk;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Nuwa.WebStack.Descriptor
{
    /// <summary>
    /// TestDescriptor is a translator to convert test class type to useful information.
    /// </summary>
    public class TestTypeDescriptor
    {
        private ITypeInfo _testClassType;
        private IEnumerable<Type> _testControllerTypes;

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
        /// The method marked by <paramref name="NuwaWebDeploymentConfigurationAttribute"/>
        /// </summary>
        public IMethodInfo WebDeployConfigMethod
        {
            get
            {
                return GetDesignatedMethod<NuwaWebDeploymentConfigurationAttribute>();
            }
        }

        /// <summary>
        /// The reflect types of the test api controllers this test depends on.
        /// </summary>
        public IEnumerable<Type> TestControllerTypes
        {
            get
            {
                if (_testControllerTypes == null)
                {
                    var types = _testClassType.GetCustomAttributes(typeof(NuwaTestControllerAttribute))
                                                        .Select(one => one.GetNamedArgument<Type>("ControllerType"));

                    if (types != null && types.Any())
                    {
                        _testControllerTypes = types.ToList();
                    }
                    else
                    {
                        _testControllerTypes = Enumerable.Empty<Type>();
                    }
                }

                return _testControllerTypes;
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