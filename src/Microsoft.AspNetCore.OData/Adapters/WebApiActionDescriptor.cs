﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Microsoft.AspNetCore.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi action description to OData WebApi.
    /// </summary>
    internal class WebApiActionDescriptor : IWebApiActionDescriptor
    {
        /// <summary>
        /// Gets the collection of supported HTTP methods for the descriptor.
        /// </summary>
        //private IList<ODataRequestMethod> supportedHttpMethods;

        /// <summary>
        /// The inner action wrapped by this instance.
        /// </summary>
        private ControllerActionDescriptor innerDescriptor;

        /// <summary>
        /// Initializes a new instance of the WebApiActionDescriptor class.
        /// </summary>
        /// <param name="actionDescriptor">The inner descriptor.</param>
        public WebApiActionDescriptor(ControllerActionDescriptor actionDescriptor)
        {
            if (actionDescriptor == null)
            {
                throw Error.ArgumentNull("actionDescriptor");
            }

            this.innerDescriptor = actionDescriptor;
        }

        /// <summary>
        /// Gets the name of the controller.
        /// </summary>
        public string ControllerName
        {
            get { return this.innerDescriptor.ControllerName; }
        }

        /// <summary>
        /// Gets the name of the action.
        /// </summary>
        public string ActionName
        {
            get { return this.innerDescriptor.ActionName; }
        }

        /// <summary>
        /// Returns the custom attributes associated with the action descriptor.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="inherit">true to search this action's inheritance chain to find the attributes; otherwise, false.</param>
        /// <returns>A list of attributes of type T.</returns>
        public IEnumerable<T> GetCustomAttributes<T>(bool inherit) where T : Attribute
        {
            return this.innerDescriptor.ControllerTypeInfo.GetCustomAttributes<T>(inherit);
        }

        /// <summary>
        /// Determine if the Http method is a match.
        /// </summary>
        public bool IsHttpMethodMatch(ODataRequestMethod method)
        {
            // ControllerActionDescriptor no longer contains a SupportedHttpMethods
            // property so return true, allowing all methods to potentially match.
            return true;
        }
    }
}