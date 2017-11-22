// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE1x
using System;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData;
#else
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
#endif

namespace Microsoft.Test.AspNet.OData.Factories
{
    /// <summary>
    /// A class to create ODataConventionModelBuilder.
    /// </summary>
    public class ODataConventionModelBuilderFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
        public static ODataConventionModelBuilder Create()
        {
#if !NETCORE1x
            return new ODataConventionModelBuilder();
#else
            ApplicationPartManager applicationPartManager = new ApplicationPartManager();
            AssemblyPart part = new AssemblyPart(typeof(ODataConventionModelBuilder).Assembly);
            applicationPartManager.ApplicationParts.Add(part);

            IContainerBuilder container = new DefaultContainerBuilder();
            container.AddService(ServiceLifetime.Singleton, sp => applicationPartManager);

            IServiceProvider serviceProvider = container.BuildContainer();
            return new ODataConventionModelBuilder(serviceProvider);
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use.</param>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
#if !NETCORE1x
        public static ODataConventionModelBuilder Create(HttpConfiguration configuration)
        {
            return new ODataConventionModelBuilder(configuration);
        }
#else
        public static ODataConventionModelBuilder Create(IRouteBuilder routeBuilder)
        {
            return new ODataConventionModelBuilder(routeBuilder.ServiceProvider);
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use.</param>
        /// <param name="isQueryCompositionMode">The value for ModelAliasingEnabled.</param>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
#if !NETCORE1x
        public static ODataConventionModelBuilder Create(HttpConfiguration configuration, bool isQueryCompositionMode)
        {
            return new ODataConventionModelBuilder(configuration, isQueryCompositionMode);
        }
#else
        public static ODataConventionModelBuilder Create(IRouteBuilder routeBuilder, bool isQueryCompositionMode)
        {
            return new ODataConventionModelBuilder(routeBuilder.ServiceProvider, isQueryCompositionMode);
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="modelAliasing">The value for ModelAliasingEnabled.</param>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
        public static ODataConventionModelBuilder CreateWithModelAliasing(bool modelAliasing)
        {
            ODataConventionModelBuilder builder = Create();
            builder.ModelAliasingEnabled = modelAliasing;
            return builder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use.</param>
        /// <param name="modelAliasing">The value for ModelAliasingEnabled.</param>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
#if !NETCORE1x
        public static ODataConventionModelBuilder CreateWithModelAliasing(HttpConfiguration configuration, bool modelAliasing)
        {
            return new ODataConventionModelBuilder(configuration) { ModelAliasingEnabled = modelAliasing };
        }
#else
        public static ODataConventionModelBuilder CreateWithModelAliasing(IRouteBuilder routeBuilder, bool modelAliasing)
        {
            return new ODataConventionModelBuilder(routeBuilder.ServiceProvider) { ModelAliasingEnabled = modelAliasing };
        }
#endif
    }
}
