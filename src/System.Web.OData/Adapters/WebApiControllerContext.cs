// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.UriParser;
using Microsoft.OData.WebApi.Interfaces;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.OData.Extensions;

namespace System.Web.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi controller context to OData WebApi.
    /// </summary>
    public class WebApiControllerContext : IWebApiControllerContext
    {
        /// <summary>
        /// Initializes a new instance of the WebApiControllerContext class.
        /// </summary>
        /// <param name="controllerContext">The inner context.</param>
        public WebApiControllerContext(HttpControllerContext controllerContext)
        {
            this.InnerContext = controllerContext;
            this.Request = new WebApiRequestMessage(controllerContext.Request);
        }

        /// <summary>
        /// The inner context wrapped by this instance.
        /// </summary>
        public HttpControllerContext InnerContext { get; private set; }

        /// <summary>
        /// Gets the request.
        /// </summary>
        public IWebApiRequestMessage Request { get; private set; }

        /// <summary>
        /// Gets the route data.
        /// </summary>
        public IDictionary<string, object> RouteData
        { 
            get { return this.InnerContext.RouteData.Values; }
        }
    }
}
