// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE1x
using System;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;
using Microsoft.Test.AspNet.OData.TestCommon;
#else
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.Test.AspNet.OData.TestCommon;
using Moq;
#endif

namespace Microsoft.Test.AspNet.OData.Factories
{
    /// <summary>
    /// A class to create ODataConventionModelBuilder.
    /// </summary>
    public class RoutingConfigurationFactory
    {
        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
#if !NETCORE1x
        public static HttpConfiguration Create()
        {
            return new HttpConfiguration();
        }
#else
        public static IRouteBuilder Create()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions();
            serviceCollection.AddRouting();
            serviceCollection.AddOData();

            var mockAction = new Mock<ActionDescriptor>();
            ActionDescriptor actionDescriptor = mockAction.Object;

            var mockActionSelector = new Mock<IActionSelector>();
            mockActionSelector
                .Setup(a => a.SelectCandidates(It.IsAny<RouteContext>()))
                .Returns(new ActionDescriptor[] { actionDescriptor });

            mockActionSelector
                .Setup(a => a.SelectBestCandidate(It.IsAny<RouteContext>(), It.IsAny<IReadOnlyList<ActionDescriptor>>()))
                .Returns(actionDescriptor);

            var mockInvoker = new Mock<IActionInvoker>();
            mockInvoker.Setup(i => i.InvokeAsync())
                .Returns(Task.FromResult(true));

            var mockInvokerFactory = new Mock<IActionInvokerFactory>();
            mockInvokerFactory.Setup(f => f.CreateInvoker(It.IsAny<ActionContext>()))
                .Returns(mockInvoker.Object);

            var mockLoggerFactory = new Mock<Microsoft.Extensions.Logging.ILoggerFactory>();

            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");

            IApplicationBuilder appBuilder = new ApplicationBuilder(serviceCollection.BuildServiceProvider());
            IRouteBuilder routeBuilder = new RouteBuilder(appBuilder);
            routeBuilder.DefaultHandler = new MvcRouteHandler(
                mockInvokerFactory.Object,
                mockActionSelector.Object,
                diagnosticSource,
                mockLoggerFactory.Object,
                new ActionContextAccessor());

            return routeBuilder;
        }
#endif

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
#if !NETCORE1x
        internal static HttpConfiguration CreateWithRootContainer(Action<IContainerBuilder> configureAction = null)
        {
            HttpConfiguration configuration = Create();

            string routeName = Microsoft.Test.AspNet.OData.Formatter.HttpRouteCollectionExtensions.RouteName;
            configuration.CreateODataRootContainer(routeName, configureAction);

            return configuration;
        }
#else
        internal static IRouteBuilder CreateWithRootContainer(Action<IContainerBuilder> configureAction)
        {
            IRouteBuilder builder = Create();

            string routeName = Microsoft.Test.AspNet.OData.Formatter.HttpRouteCollectionExtensions.RouteName;

            return builder;
        }
#endif

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
#if !NETCORE1x
        internal static HttpConfiguration CreateWithAssemblyResolver(MockAssembly assembly)
        {
            HttpConfiguration configuration = Create();

            TestAssemblyResolver resolver = new TestAssemblyResolver(assembly);
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            return configuration;
        }
#else
        internal static IRouteBuilder CreateWithAssemblyResolver(MockAssembly assembly)
        {
            IRouteBuilder builder = Create();

            ApplicationPartManager applicationPartManager = builder.ApplicationBuilder.ApplicationServices.GetRequiredService<ApplicationPartManager>();
            AssemblyPart part = new AssemblyPart(assembly);
            applicationPartManager.ApplicationParts.Add(part);

            return builder;
        }
#endif

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
#if !NETCORE1x
        internal static HttpConfiguration CreateWithAssemblyResolver(params Type[] types)
        {
            HttpConfiguration configuration = Create();

            TestAssemblyResolver resolver = new TestAssemblyResolver(types);
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            return configuration;
        }
#else
        internal static IRouteBuilder CreateWithAssemblyResolver(params Type[] types)
        {
            IRouteBuilder builder = Create();

            ApplicationPartManager applicationPartManager = builder.ApplicationBuilder.ApplicationServices.GetRequiredService<ApplicationPartManager>();
            AssemblyPart part = new AssemblyPart(new MockAssembly(types));
            applicationPartManager.ApplicationParts.Add(part);

            return builder;
        }
#endif

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
#if !NETCORE1x
        public static HttpConfiguration CreateFromControllers(params Type[] controllers)
        {
            var resolver = new TestAssemblyResolver(new MockAssembly(controllers));
            var configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);
            configuration.Count().OrderBy().Filter().Expand().MaxTop(null);
            return configuration;
        }
#else
        public static IRouteBuilder CreateFromControllers(params Type[] controllers)
        {
            IRouteBuilder builder = Create();

            ApplicationPartManager applicationPartManager = builder.ApplicationBuilder.ApplicationServices.GetRequiredService<ApplicationPartManager>();
            AssemblyPart part = new AssemblyPart(new MockAssembly(controllers));
            applicationPartManager.ApplicationParts.Add(part);

            return builder;
        }
#endif
    }
}