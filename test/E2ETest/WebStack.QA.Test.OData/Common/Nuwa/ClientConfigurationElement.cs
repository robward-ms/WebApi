// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net.Http;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Nuwa
{
    internal static class ClientConfigurationElement
    {
        /// <summary>
        /// Create an http client according to configuration and host strategy
        /// </summary>
        /// <param name="testClass">the test class under test</param>
        public static void SetHttpclient(Type testClassType, object testClassInstance, NuwaTestCase testCommand)
        {
            // set the HttpClient if necessary
            var clientPrpt = testClassType.GetProperties()
                .Where(prop => { return prop.GetCustomAttributes(typeof(NuwaHttpClientAttribute), false).Length == 1; })
                .FirstOrDefault();

            if (clientPrpt == null || NuwaHttpClientAttribute.Verify(clientPrpt) == false)
            {
                return;
            }

            // create client assign to property
            clientPrpt.SetValue(testClassInstance, new HttpClient(), null);
        }
    }
}
