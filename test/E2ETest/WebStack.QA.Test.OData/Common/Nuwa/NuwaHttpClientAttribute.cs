// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Reflection;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Nuwa
{
    /// <summary>
    /// NuwaHttpClientAttribute is used to mark a property in the test class. To the
    /// property, test framework will assign HttpClient instance. The framework created
    /// HttpClient instance is used in those scenarios that specific tuning needs to
    /// be done to the client.
    /// 
    /// The property's type must be <paramref name="System.Net.Http.HttpClient"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class NuwaHttpClientAttribute : Attribute
    {
        public static bool Verify(PropertyInfo property)
        {
            if (!property.PropertyType.IsAssignableFrom(typeof(HttpClient)))
            {
                return false;
            }

            if (!property.CanWrite)
            {
                return false;
            }

            return true;
        }
    }
}
