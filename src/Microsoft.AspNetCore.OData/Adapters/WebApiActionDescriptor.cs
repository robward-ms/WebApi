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
    /// Adapter class to convert Asp.Net WebApi action description to OData WebApi.
    /// </summary>
    public class WebApiActionDescriptor : IWebApiActionDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the WebApiActionDescriptor class.
        /// </summary>
        /// <param name="actionDescriptor">The inner descriptor.</param>
        public WebApiActionDescriptor(ControllerActionDescriptor actionDescriptor)
        {
            this.InnerDescriptor = actionDescriptor;
            this.ControllerDescriptor = new WebApiControllerDescriptor(actionDescriptor);
        }

        /// <summary>
        /// The inner action wrapped by this instance.
        /// </summary>
        public ControllerActionDescriptor InnerDescriptor { get; private set; }

        /// <summary>
        /// Gets the name of the action.
        /// </summary>
        public string ActionName
        {
            get { return this.InnerDescriptor.ActionName; }
        }

        /// <summary>
        /// Gets the information that describes the controller of the action.
        /// </summary>
        public IWebApiControllerDescriptor ControllerDescriptor { get; private set; }

        /// <summary>
        /// Returns the custom attributes associated with the action descriptor.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="inherit">true to search this action's inheritance chain to find the attributes; otherwise, false.</param>
        /// <returns>A list of attributes of type T.</returns>
        public IEnumerable<T> GetCustomAttributes<T>(bool inherit) where T : Attribute
        {
            return this.InnerDescriptor.ControllerTypeInfo.GetCustomAttributes<T>(inherit);
        }

        /// <summary>
        /// Determine if the Http method is a match.
        /// </summary>
        public bool IsHttpMethodMatch(string method)
        {
            // ControllerActionDescriptor no longer contains a SupportedHttpMethods
            // property so return true, allowing all methods to potentially match.
            return true;
        }
    }
}
