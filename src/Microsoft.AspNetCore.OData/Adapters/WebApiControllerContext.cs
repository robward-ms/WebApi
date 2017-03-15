// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.WebApi.Interfaces;
using Microsoft.OData.WebApi.Routing.Conventions;

namespace Microsoft.AspNetCore.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi controller context to OData WebApi.
    /// </summary>
    public class WebApiControllerContext : IWebApiControllerContext
    {
        /// <summary>
        /// Initializes a new instance of the WebApiControllerContext class.
        /// </summary>
        /// <param name="routeContext">The inner context.</param>
        /// <param name="controllerResult">The selected controller result.</param>
        public WebApiControllerContext(RouteContext routeContext, SelectControllerResult controllerResult)
        {
            this.InnerContext = routeContext;
            this.ControllerResult = controllerResult;
            this.Request = new WebApiRequestMessage(routeContext.HttpContext.Request);
        }

        /// <summary>
        /// The inner context wrapped by this instance.
        /// </summary>
        public RouteContext InnerContext { get; private set; }

        /// <summary>
        /// The selected controller result.
        /// </summary>
        public SelectControllerResult ControllerResult { get; private set; }

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
