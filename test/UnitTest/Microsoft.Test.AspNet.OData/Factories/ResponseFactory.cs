﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Net;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData;
#else
using System.Net;
using System.Net.Http;
#endif

namespace Microsoft.Test.AspNet.OData.Factories
{
    /// <summary>
    /// A class to create HttpResponse[Message].
    /// </summary>
    public class ResponseFactory
    {
        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
#if NETCORE
        public static HttpResponse Create(HttpStatusCode statusCode)
        {
            // Add the options services.
            IRouteBuilder config = RoutingConfigurationFactory.CreateWithRootContainer("OData");

            // Create a new context and assign the services.
            HttpContext context = new DefaultHttpContext();
            context.RequestServices = config.ServiceProvider;
            //context.ODataFeature().RequestContainer = provider;

            // Get response and return it.
            HttpResponse response = context.Response;
            response.StatusCode = (int)statusCode;
            return response;
        }
#else
        public static HttpResponseMessage Create(HttpStatusCode statusCode)
        {
            var response = new HttpResponseMessage(statusCode);
            return response;
        }
#endif
    }
}