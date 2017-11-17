// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Net.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.TestCommon;
#else
using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.TestCommon;
#endif

namespace Microsoft.Test.AspNet.OData.Factories
{
    /// <summary>
    /// Factory for creating a test servers.
    /// </summary>
    public class TestServerFactory
    {
#if NETCORE
        /// <summary>
        /// Create an TestServer.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="controllers">The controllers to use.</param>
        /// <param name="getModelFunction">A function to get the model.</param>
        /// <returns>An TestServer.</returns>
        public static TestServer Create(
            string routeName,
            string routePrefix,
            Type[] controllers,
            Func<IRouteBuilder,IEdmModel> getModelFunction)
        {
            IWebHostBuilder builder = WebHost.CreateDefaultBuilder().Configure(app =>
            {
                app.UseMvc((routeBuilder) =>
                {
                    routeBuilder.MapODataServiceRoute(routeName, routePrefix, getModelFunction(routeBuilder));

                    ApplicationPartManager applicationPartManager = routeBuilder.ApplicationBuilder.ApplicationServices.GetRequiredService<ApplicationPartManager>();
                    AssemblyPart part = new AssemblyPart(new MockAssembly(controllers));
                    applicationPartManager.ApplicationParts.Add(part);
                });
            });

            return new TestServer(builder);
        }

        /// <summary>
        /// Create an HttpClient from a server.
        /// </summary>
        /// <param name="server">The TestServer.</param>
        /// <returns>An HttpClient.</returns>
        public static HttpClient CreateClient(TestServer server)
        {
            return server.CreateClient();
        }
#else
        /// <summary>
        /// Create an HttpServer.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="controllers">The controllers to use.</param>
        /// <param name="getModelFunction">A function to get the model.</param>
        /// <returns>An HttpServer.</returns>
        public static HttpServer Create(
            string routeName,
            string routePrefix,
            Type[] controllers,
            Func<HttpConfiguration, IEdmModel> getModelFunction)
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.MapODataServiceRoute(routeName, routePrefix, getModelFunction(configuration));

            TestAssemblyResolver resolver = new TestAssemblyResolver(new MockAssembly(controllers));
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            HttpServer server = new HttpServer(configuration);
            configuration.EnsureInitialized();

            return server;
        }

        /// <summary>
        /// Create an HttpClient from a server.
        /// </summary>
        /// <param name="server">The HttpServer.</param>
        /// <returns>An HttpClient.</returns>
        public static HttpClient CreateClient(HttpServer server)
        {
            return new HttpClient(server);
        }
#endif
    }
}
