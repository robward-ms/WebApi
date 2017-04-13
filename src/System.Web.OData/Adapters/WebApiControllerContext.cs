// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.OData.WebApi.Common;
using Microsoft.OData.WebApi.Interfaces;
using Microsoft.OData.WebApi.Routing.Conventions;

namespace System.Web.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi controller context to OData WebApi.
    /// </summary>
    public class WebApiControllerContext : IWebApiControllerContext
    {
        /// <summary>
        /// The inner context wrapped by this instance.
        /// </summary>
        private HttpControllerContext innerContext;

        /// <summary>
        /// Initializes a new instance of the WebApiControllerContext class.
        /// </summary>
        /// <param name="controllerContext">The inner context.</param>
        /// <param name="controllerResult">The selected controller result.</param>
        public WebApiControllerContext(HttpControllerContext controllerContext, SelectControllerResult controllerResult)
        {
            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            this.innerContext = controllerContext;
            this.ControllerResult = controllerResult;

            HttpRequestMessage request = controllerContext.Request;
            if (request != null)
            {
                this.Request = new WebApiRequestMessage(request);
            }
        }

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
            get { return this.innerContext.RouteData.Values; }
        }
    }
}
