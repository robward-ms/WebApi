// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
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

        public WebRouteConfiguration AddControllers(params Type[] controllers)
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
            return this;
        }

        /// <summary>
        /// Gets or sets the ReferenceLoopHandling property on the Json formatter.
        /// </summary>
        public ReferenceLoopHandling JsonReferenceLoopHandling
        {
            get { return ReferenceLoopHandling.Error; }
            set { }
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
