// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.WebApi.Interfaces;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Extensions;

namespace System.Web.OData.Adapters
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
        /// <param name="controllerDescriptor">The parent controller descriptor.</param>
        public WebApiActionDescriptor(HttpActionDescriptor actionDescriptor, WebApiControllerDescriptor controllerDescriptor)
        {
            this.InnerDescriptor = actionDescriptor;
            this.ControllerDescriptor = controllerDescriptor;

            this.SupportedHttpMethods = new List<string>();
            foreach (HttpMethod method in actionDescriptor.SupportedHttpMethods)
            {
                this.SupportedHttpMethods.Add(method.Method);
            }
        }

        /// <summary>
        /// The inner action wrapped by this instance.
        /// </summary>
        public HttpActionDescriptor InnerDescriptor { get; private set; }

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
        /// Gets the collection of supported HTTP methods for the descriptor.
        /// </summary>
        public IList<string> SupportedHttpMethods { get; private set; }

        /// <summary>
        /// Returns the custom attributes associated with the action descriptor.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="inherit">true to search this action's inheritance chain to find the attributes; otherwise, false.</param>
        /// <returns>A list of attributes of type T.</returns>
        public IList<T> GetCustomAttributes<T>(bool inherit) where T : class
        {
            return this.InnerDescriptor.GetCustomAttributes<T>(inherit);
        }

        /// <summary>
        /// Get the service provider for OData for a given route.
        /// </summary>
        /// <param name="routeName">The route name for which toi get a service provider.</param>
        /// <returns>A service provider for OData for a given route</returns>
        public IServiceProvider GetODataRootContainer(string routeName)
        {
            return this.InnerDescriptor.Configuration.GetODataRootContainer(routeName);
        }
    }
}
