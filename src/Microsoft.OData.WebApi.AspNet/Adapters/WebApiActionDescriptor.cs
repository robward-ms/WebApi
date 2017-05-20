// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.OData.WebApi.Common;
using Microsoft.OData.WebApi.Interfaces;

namespace Microsoft.OData.WebApi.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi action description to OData WebApi.
    /// </summary>
    internal class WebApiActionDescriptor : IWebApiActionDescriptor
    {
        /// <summary>
        /// Gets the collection of supported HTTP methods for the descriptor.
        /// </summary>
        private IList<string> supportedHttpMethods;

        /// <summary>
        /// The inner action wrapped by this instance.
        /// </summary>
        private HttpActionDescriptor innerDescriptor;

        /// <summary>
        /// Initializes a new instance of the WebApiActionDescriptor class.
        /// </summary>
        /// <param name="actionDescriptor">The inner descriptor.</param>
        public WebApiActionDescriptor(HttpActionDescriptor actionDescriptor)
            ////WebApiControllerDescriptor controllerDescriptor)
        {
            if (actionDescriptor == null)
            {
                throw Error.ArgumentNull("actionDescriptor");
            }

            this.innerDescriptor = actionDescriptor;

            this.supportedHttpMethods = new List<string>();
            foreach (HttpMethod method in actionDescriptor.SupportedHttpMethods)
            {
                this.supportedHttpMethods.Add(method.Method);
            }
        }

        /// <summary>
        /// Gets the name of the controller.
        /// </summary>
        public string ControllerName
        {
            get { return this.innerDescriptor.ControllerDescriptor.ControllerName; }
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
            return this.innerDescriptor.GetCustomAttributes<T>(inherit);
        }

        /// <summary>
        /// Determine if the Http method is a match.
        /// </summary>
        public bool IsHttpMethodMatch(string method)
        {
            return this.supportedHttpMethods.Contains(method);
        }
    }
}
