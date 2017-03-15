// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.AspNetCore.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi controller description to OData WebApi.
    /// </summary>
    public class WebApiControllerDescriptor : IWebApiControllerDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the WebApiControllerDescriptor class.
        /// </summary>
        /// <param name="descriptor">The inner descriptor.</param>
        public WebApiControllerDescriptor(ControllerActionDescriptor descriptor)
        {
            this.InnerDescriptor = descriptor;
        }

        /// <summary>
        /// The inner resolver wrapped by this instance.
        /// </summary>
        public ControllerActionDescriptor InnerDescriptor { get; private set; }

        /// <summary>
        /// Gets the name of the controller.
        /// </summary>
        public string ControllerName
        {
            get { return this.InnerDescriptor.ControllerName; }
        }

        /// <summary>
        /// Gets the type of the controller.
        /// </summary>
        public Type ControllerType
        {
            get { return this.InnerDescriptor.ControllerTypeInfo.BaseType; }
        }

        /// <summary>
        /// Returns a collection of attributes that can be assigned to <typeparamref name="T" /> for this descriptor's controller. 
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="inherit">true to search this action's inheritance chain to find the attributes; otherwise, false.</param>
        /// <returns>A list of attributes of type T.</returns>
        public IEnumerable<T> GetCustomAttributes<T>(bool inherit) where T : Attribute
        {
            return this.InnerDescriptor.ControllerTypeInfo.GetCustomAttributes<T>(inherit);
        }
    }
}
