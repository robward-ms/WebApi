// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.WebApi.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace System.Web.OData.Adapters
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
        public WebApiControllerDescriptor(HttpControllerDescriptor descriptor)
        {
            this.InnerDescriptor = descriptor;
        }

        /// <summary>
        /// The inner resolver wrapped by this instance.
        /// </summary>
        public HttpControllerDescriptor InnerDescriptor { get; private set; }

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
            get { return this.InnerDescriptor.ControllerType; }
        }

        /// <summary>
        /// Returns a collection of actions associated with the controller.
        /// </summary>
        /// <returns>A collection of actions associated with the controller</returns>
        public IWebApiActionDescriptor[] GetActions()
        {
            IHttpActionSelector actionSelector = this.InnerDescriptor.Configuration.Services.GetActionSelector();
            ILookup<string, HttpActionDescriptor> actionMapping = actionSelector.GetActionMapping(this.InnerDescriptor);
            IEnumerable<HttpActionDescriptor> actions = actionMapping.SelectMany(a => a);
            IEnumerable<IWebApiActionDescriptor> convertedActions =
                actions.Select(a => new WebApiActionDescriptor(a, this));

            return convertedActions.ToArray();
        }

        /// <summary>
        /// Returns a collection of attributes that can be assigned to <typeparamref name="T" /> for this descriptor's controller. 
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="inherit">true to search this action's inheritance chain to find the attributes; otherwise, false.</param>
        /// <returns>A list of attributes of type T.</returns>
        public IList<T> GetCustomAttributes<T>(bool inherit) where T : class
        {
            return this.InnerDescriptor.GetCustomAttributes<T>(inherit);
        }
    }
}
