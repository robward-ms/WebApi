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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData;
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
    /// Factory for creating a TestServer
    /// </summary>
    public class TestServerFactory
    {
#if NETCORE
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

            var server = new TestAspNetCoreServer(builder);
            return new TestServer(server);
        }
#else
        public static TestServer Create(
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

            return new TestServer(server);
        }
#endif
    }

    /// <summary>
    /// An abstracted TestServer
    /// </summary>
    public class TestServer
    {
#if NETCORE
        private TestAspNetCoreServer innerServer;

        public TestServer(TestAspNetCoreServer innerServer)
        {
        }

        public HttpClient Client
        {
            get { return innerServer.CreateClient(); }
        }
#else
        private HttpServer innerServer;

        public TestServer(HttpServer innerServer)
        {
            this.innerServer = innerServer;
        }

        public HttpClient Client
        {
            get { return new HttpClient(innerServer); }
        }
#endif
    }
}
