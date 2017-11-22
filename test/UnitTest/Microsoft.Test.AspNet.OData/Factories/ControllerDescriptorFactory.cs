// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE1x
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
#else
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Controllers;
#endif

namespace Microsoft.Test.AspNet.OData.Factories
{
    /// <summary>
    /// A class to create [Http]ControllerDescriptor.
    /// </summary>
    public class ControllerDescriptorFactory
    {
        /// <summary>
        /// Initializes a new instance of the [Http]ControllerDescriptor class.
        /// </summary>
        /// <returns>A new instance of the [Http]ControllerDescriptor  class.</returns>
#if NETCORE1x
        public static ControllerActionDescriptor Create()
        {
            return new ControllerActionDescriptor();
        }
#else
        public static HttpControllerDescriptor Create()
        {
            return new HttpControllerDescriptor();
        }
#endif

        /// <summary>
        /// Initializes a new instance of the [Http]ControllerDescriptor class.
        /// </summary>
        /// <returns>A new instance of the [Http]ControllerDescriptor  class.</returns>
#if NETCORE1x
        public static ControllerActionDescriptor Create(IRouteBuilder routeBuilder, string name, Type controllerType)
        {
            ControllerActionDescriptor descriptor = new ControllerActionDescriptor();
            descriptor.ControllerName = name;
            descriptor.ControllerTypeInfo = controllerType.GetTypeInfo();

            return descriptor;
        }
#else
        public static HttpControllerDescriptor Create(HttpConfiguration configuration, string name, Type controllerType)
        {
            return new HttpControllerDescriptor(configuration, name, controllerType);
        }
#endif

        /// <summary>
        /// Initializes a new collection of the [Http]ControllerDescriptor class.
        /// </summary>
        /// <returns>A new collection of the [Http]ControllerDescriptor  class.</returns>
#if NETCORE1x
        public static IEnumerable<ControllerActionDescriptor> CreateCollection()
        {
            return new ControllerActionDescriptor[0];
        }
#else
        public static IEnumerable<HttpControllerDescriptor> CreateCollection()
        {
            return new HttpControllerDescriptor[0];
        }
#endif
    }
}
