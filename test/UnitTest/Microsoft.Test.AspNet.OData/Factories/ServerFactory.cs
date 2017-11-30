// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
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
            Func<IRouteBuilder, IEdmModel> getModelFunction)
        {
            return Create(routeName, routePrefix, controllers, getModelFunction, null);
        }

        /// <summary>
        /// Create an TestServer.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <param name="routeName">The route name.</param>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="controllers">The controllers to use.</param>
        /// <param name="getModelFunction">A function to get the model.</param>
        /// <returns>An TestServer.</returns>
        public static TestServer CreateWithRoute(
            string route,
            string routeName,
            string routePrefix,
            Type[] controllers,
            Func<IRouteBuilder,IEdmModel> getModelFunction)
        {
            return Create(routeName, routePrefix, controllers, getModelFunction,
                (routeBuilder) =>
                {
                    // Get constraint resolver.
                    IInlineConstraintResolver inlineConstraintResolver = routeBuilder
                        .ServiceProvider
                        .GetRequiredService<IInlineConstraintResolver>();

                    //// Add route.
                    //routeBuilder.Routes.Add(new Route(routeBuilder.DefaultHandler, route, inlineConstraintResolver));
                });
        }

        /// <summary>
        /// Create an TestServer.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <param name="routeName">The route name.</param>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="controllers">The controllers to use.</param>
        /// <param name="getModelFunction">A function to get the model.</param>
        /// <param name="configRouteAction">An action to apply to routing config.</param>
        /// <returns>An TestServer.</returns>
        private static TestServer Create(
            string routeName,
            string routePrefix,
            Type[] controllers,
            Func<IRouteBuilder, IEdmModel> getModelFunction,
            Action<IRouteBuilder> configRouteAction)
        {
            IWebHostBuilder builder = WebHost.CreateDefaultBuilder();
            builder.ConfigureServices(services =>
            {
                services.AddMvc();
                services.AddOData();
            });

            builder.Configure(app =>
            {
                app.UseMvc((routeBuilder) =>
                {
                    configRouteAction?.Invoke(routeBuilder);

                    routeBuilder.MapODataServiceRoute(routeName, routePrefix, getModelFunction(routeBuilder));

                    ApplicationPartManager applicationPartManager = routeBuilder.ApplicationBuilder.ApplicationServices.GetRequiredService<ApplicationPartManager>();
                    applicationPartManager.ApplicationParts.Clear();

                    AssemblyPart part = new AssemblyPart(new MockAssembly(controllers));
                    applicationPartManager.ApplicationParts.Add(part);

                    // Insert a custom ControllerFeatureProvider to bypass the IsPublic restriction of controllers
                    // to allow for nested controllers which are excluded by the built-in ControllerFeatureProvider.
                    applicationPartManager.FeatureProviders.Clear();
                    applicationPartManager.FeatureProviders.Add(new TestControllerFeatureProvider());
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

        private class TestControllerFeatureProvider : ControllerFeatureProvider
        {
            /// <inheritdoc />
            /// <remarks>
            /// Identical to ControllerFeatureProvider.IsController except for the typeInfo.IsPublic check.
            /// </remarks>
            protected override bool IsController(TypeInfo typeInfo)
            {
                if (!typeInfo.IsClass)
                {
                    return false;
                }

                if (typeInfo.IsAbstract)
                {
                    return false;
                }

                if (typeInfo.ContainsGenericParameters)
                {
                    return false;
                }

                if (typeInfo.IsDefined(typeof(NonControllerAttribute)))
                {
                    return false;
                }

                if (!typeInfo.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase) &&
                    !typeInfo.IsDefined(typeof(ControllerAttribute)))
                {
                    return false;
                }

                return true;
            }
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
            return Create(configuration, routeName, routePrefix, controllers, getModelFunction);
        }

        /// <summary>
        /// Create an HttpServer.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <param name="routeName">The route name.</param>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="controllers">The controllers to use.</param>
        /// <param name="getModelFunction">A function to get the model.</param>
        /// <returns>An HttpServer.</returns>
        public static HttpServer CreateWithRoute(
            string route,
            string routeName,
            string routePrefix,
            Type[] controllers,
            Func<HttpConfiguration, IEdmModel> getModelFunction)
        {
            HttpConfiguration configuration = new HttpConfiguration(new HttpRouteCollection(route));
            return Create(configuration, routeName, routePrefix, controllers, getModelFunction);
        }

        /// <summary>
        /// Create an HttpServer.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="routeName">The route name.</param>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="controllers">The controllers to use.</param>
        /// <param name="getModelFunction">A function to get the model.</param>
        /// <returns>An HttpServer.</returns>
        public static HttpServer Create(
            HttpConfiguration configuration,
            string routeName,
            string routePrefix,
            Type[] controllers,
            Func<HttpConfiguration, IEdmModel> getModelFunction)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.MapODataServiceRoute(routeName, routePrefix, getModelFunction(configuration));
            configuration.Count().OrderBy().Filter().Expand().MaxTop(null).Select();

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
