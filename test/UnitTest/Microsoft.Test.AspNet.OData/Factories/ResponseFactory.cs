// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE1x
using System;
using System.Net;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
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
#if !NETCORE1x
        public static HttpResponseMessage Create(HttpStatusCode statusCode)
        {
            var response = new HttpResponseMessage(statusCode);
            return response;
        }
#else
        public static HttpResponse Create(HttpStatusCode statusCode)
        {
            IContainerBuilder builder = new DefaultContainerBuilder();
            builder.AddService<ODataOptions, ODataOptions>(Microsoft.OData.ServiceLifetime.Singleton);
            builder.AddService<DefaultQuerySettings, DefaultQuerySettings>(Microsoft.OData.ServiceLifetime.Singleton);

            HttpContext context = new DefaultHttpContext();

            IServiceProvider provider = builder.BuildContainer();
            context.ODataFeature().RequestContainer = provider;
            context.RequestServices = provider;

            HttpResponse response = context.Response;
            response.StatusCode = (int)statusCode;

            return response;
        }
#endif
    }
}
