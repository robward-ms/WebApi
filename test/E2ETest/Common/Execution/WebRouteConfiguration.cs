// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.Test.AspNet.OData.TestCommon;
using Newtonsoft.Json;
#else
using System;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Builder;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json;
using Microsoft.AspNet.OData.Routing.Conventions;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Common.Execution
{
    /// <summary>
    /// And abstracted version of web configuration allow callers to configure AspNet or AspNetCore.
    /// </summary>
#if NETCORE
    public class WebRouteConfiguration : IRouteBuilder
    {
        private IRouteBuilder routeBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRouteConfiguration"/> class.
        /// </summary>
        public WebRouteConfiguration(IRouteBuilder routeBuilder)
        {
            this.routeBuilder = routeBuilder;
        }

        /// <summary>
        /// Implement IRouteBuilder and pass to the base class.
        /// </summary>
        public IApplicationBuilder ApplicationBuilder => routeBuilder.ApplicationBuilder;
        IRouter IRouteBuilder.DefaultHandler { get => routeBuilder.DefaultHandler; set { routeBuilder.DefaultHandler = value; } }
        public IServiceProvider ServiceProvider => routeBuilder.ServiceProvider;
        public IList<IRouter> Routes => routeBuilder.Routes;
        public IRouter Build() => routeBuilder.Build();

    /// <summary>
    /// Gets or sets the ReferenceLoopHandling property on the Json formatter.
    /// </summary>
    public ReferenceLoopHandling JsonReferenceLoopHandling
        {
            get { return ReferenceLoopHandling.Error; }
            set { }
        }

        /// <summary>
        /// Ensure the configuration is initialized.
        /// </summary>
        public void EnsureInitialized()
        {
            // This is a no-op on AspNetCore.
        }

        /// <summary>
        /// Enable dependency injection for non-OData routes.
        /// </summary>
        public void EnableDependencyInjection()
        {
            // This is a no-op on AspNetCore.
        }

        /// <summary>
        /// Enable HTTP route.
        /// </summary>
        public void MapHttpRoute(string name, string template)
        {
            this.MapRoute(name, template);
        }

        /// <summary>
        /// Enable HTTP route.
        /// </summary>
        public void MapHttpRoute(string name, string template, object defaults)
        {
            this.MapRoute(name, template, defaults);
        }

        /// <summary>
        /// Create an <see cref="ODataConventionModelBuilder"/>.
        /// </summary>
        /// <returns>An <see cref="ODataConventionModelBuilder"/></returns>
        public ODataConventionModelBuilder CreateConventionModelBuilder()
        {
            return new ODataConventionModelBuilder(routeBuilder.ServiceProvider);
        }

        /// <summary>
        /// Create an <see cref="AttributeRoutingConvention"/>.
        /// </summary>
        /// <returns>An <see cref="AttributeRoutingConvention"/></returns>
        public AttributeRoutingConvention CreateAttributeRoutingConvention(string name = "AttributeRouting")
        {
            // Since we could be building the container, we must supply the path handler.
            return new AttributeRoutingConvention(name, routeBuilder.ServiceProvider, new DefaultODataPathHandler());
        }

        /// <summary>
        /// Add a list of controllers to be discovered by the application.
        /// </summary>
        /// <param name="controllers"></param>
        public void AddControllers(params Type[] controllers)
        {
            ApplicationPartManager applicationPartManager =
                routeBuilder.ApplicationBuilder.ApplicationServices.GetRequiredService<ApplicationPartManager>();

            // Strip out all the IApplicationPartTypeProvider parts.
            IList<ApplicationPart> parts = applicationPartManager.ApplicationParts;
            IList<ApplicationPart> nonAssemblyParts = parts.Where(p => p.GetType() != typeof(IApplicationPartTypeProvider)).ToList();
            applicationPartManager.ApplicationParts.Clear();
            applicationPartManager.ApplicationParts.Concat(nonAssemblyParts);

            // Add a new AssemblyPart with the controllers.
            AssemblyPart part = new AssemblyPart(new TestAssembly(controllers));
            applicationPartManager.ApplicationParts.Add(part);
        }

        /// <summary>
        /// Enables query support for actions with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return
        /// type. To avoid processing unexpected or malicious queries, use the validation settings on
        /// <see cref="EnableQueryAttribute"/> to validate incoming queries. For more information, visit
        /// http://go.microsoft.com/fwlink/?LinkId=279712.
        /// </summary>
        public void AddODataQueryFilter()
        {
            // This is enabled by default.
        }

        /// <summary>
        /// Enables query support for actions with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return
        /// type. To avoid processing unexpected or malicious queries, use the validation settings on
        /// <see cref="EnableQueryAttribute"/> to validate incoming queries. For more information, visit
        /// http://go.microsoft.com/fwlink/?LinkId=279712.
        /// </summary>
        /// <param name="services">The services collection.</param>
        /// <param name="queryFilter">The action filter that executes the query.</param>
        public void AddODataQueryFilter(IActionFilter queryFilter)
        {
#if !NETCORE
#endif
        }

        /// <summary>
        /// Add an <see cref="ETagMessageHandler"/> to the configuration.
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public void AddETagMessageHandler(ETagMessageHandler handler)
        {
            //configuration.MessageHandlers.Add(new ETagMessageHandler());
        }
    }
#else
        public class WebRouteConfiguration : HttpConfiguration
    {
        public WebRouteConfiguration AddControllers(params Type[] controllers)
        {
            this.Services.Replace(
                typeof(IAssembliesResolver),
                new TestAssemblyResolver(controllers));

            return this;
        }

        /// <summary>
        /// Enable http route.
        /// </summary>
        public void MapHttpRoute(string name, string template)
        {
            this.Routes.MapHttpRoute(name, template);
        }

        /// <summary>
        /// Enable http route.
        /// </summary>
        public void MapHttpRoute(string name, string template, object defaults)
        {
            this.Routes.MapHttpRoute(name, template, defaults);
        }

        /// <summary>
        /// Add an <see cref="ETagMessageHandler"/> to the configuration.
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public void AddETagMessageHandler(ETagMessageHandler handler)
        {
            configuration.MessageHandlers.Add(new ETagMessageHandler());
        }

        /// <summary>
        /// Create an <see cref="ODataConventionModelBuilder"/>.
        /// </summary>
        /// <returns>An <see cref="ODataConventionModelBuilder"/></returns>
        public ODataConventionModelBuilder CreateConventionModelBuilder()
        {
            return new ODataConventionModelBuilder(this);
        }

        /// <summary>
        /// Create an <see cref="AttributeRoutingConvention"/>.
        /// </summary>
        /// <returns>An <see cref="AttributeRoutingConvention"/></returns>
        public AttributeRoutingConvention CreateAttributeRoutingConvention(string name = "AttributeRouting")
        {
            return new AttributeRoutingConvention(name, this);
        }

        /// <summary>
        /// Create an <see cref="DefaultODataBatchHandler"/>.
        /// </summary>
        /// <returns>An <see cref="DefaultODataBatchHandler"/></returns>
        public DefaultODataBatchHandler CreateDefaultODataBatchHandler()
        {
            return new DefaultODataBatchHandler(this.GetHttpServer());
        }

        /// <summary>
        /// Create an <see cref="UnbufferedODataBatchHandler"/>.
        /// </summary>
        /// <returns>An <see cref="UnbufferedODataBatchHandler"/></returns>
        public UnbufferedODataBatchHandler CreateUnbufferedODataBatchHandler()
        {
            return new UnbufferedODataBatchHandler(this.GetHttpServer());
        }

        /// <summary>
        /// Gets or sets the ReferenceLoopHandling property on the Json formatter.
        /// </summary>
        public ReferenceLoopHandling JsonReferenceLoopHandling
        {
            get { return this.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling; }
            set { this.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = value; }
        }

        /// <summary>
        /// Gets or sets the ReferenceLoopHandling property on the Json formatter.
        /// </summary>
        public int MaxReceivedMessageSize
        {
            get { return 0; }
            set { }
            //var selfHostConfig = configuration as HttpSelfHostConfiguration;
            //if (selfHostConfig != null)
            //{
            //    selfHostConfig.MaxReceivedMessageSize = selfHostConfig.MaxBufferSize = int.MaxValue;
            //}
        }
    }
#endif
}
