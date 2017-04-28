// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Controllers;
using Microsoft.OData.WebApi.Common;
using Microsoft.OData.WebApi.Interfaces;

namespace System.Web.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi controller description to OData WebApi.
    /// </summary>
    public class WebApiControllerDescriptor : IWebApiControllerDescriptor
    {
        /// <summary>
        /// The inner resolver wrapped by this instance.
        /// </summary>
        private HttpControllerDescriptor innerDescriptor;

        /// <summary>
        /// Initializes a new instance of the WebApiControllerDescriptor class.
        /// </summary>
        /// <param name="descriptor">The inner descriptor.</param>
        public WebApiControllerDescriptor(HttpControllerDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw Error.ArgumentNull("descriptor");
            }

            this.innerDescriptor = descriptor;
        }

        /// <summary>
        /// Gets the name of the controller.
        /// </summary>
        public string ControllerName
        {
            get { return this.innerDescriptor.ControllerName; }
        }

        /// <summary>
        /// Gets the type of the controller.
        /// </summary>
        public Type ControllerType
        {
            get { return this.innerDescriptor.ControllerType; }
        }

        /// <summary>
        /// Returns a collection of attributes that can be assigned to <typeparamref name="T" /> for this descriptor's controller. 
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="inherit">true to search this action's inheritance chain to find the attributes; otherwise, false.</param>
        /// <returns>A list of attributes of type T.</returns>
        public IEnumerable<T> GetCustomAttributes<T>(bool inherit) where T : Attribute
        {
            return this.innerDescriptor.GetCustomAttributes<T>(inherit);
        }
    }
}
