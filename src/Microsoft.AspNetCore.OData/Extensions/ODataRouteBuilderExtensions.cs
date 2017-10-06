// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IRouteBuilder"/> to add OData routes.
    /// </summary>
    public static class ODataRouteBuilderExtensions
    {
        /// <summary>
        /// Maps the specified OData route and the OData route attributes.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <param name="configureAction">The configuring action to add the services to the root container.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapODataServiceRoute(this IRouteBuilder builder, string routeName,
            string routePrefix, IEdmModel model, Action<IContainerBuilder> configureAction)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            if (routeName == null)
            {
                throw Error.ArgumentNull("routeName");
            }

            // Build and configure the root container.
            // TODO: In AspNetCore, we have one container for all routes.
            //IServiceProvider rootContainer = configuration.CreateODataRootContainer(routeName, configureAction);

            // TODO: Actually need per-route containers.
            configureAction?.Invoke(new DefaultContainerBuilder(builder.ApplicationBuilder.ApplicationServices));

            // Resolve the path handler and set URI resolver to it.
            IODataPathHandler pathHandler = builder.ServiceProvider.GetRequiredService<IODataPathHandler>();

            // If settings is not on local, use the global configuration settings.
            ODataOptions options = builder.ServiceProvider.GetRequiredService<IOptions<ODataOptions>>().Value;
            if (pathHandler != null && pathHandler.UrlKeyDelimiter == null)
            {
                pathHandler.UrlKeyDelimiter = options.UrlKeyDelimiter;
            }

            // Resolve some required services and create the route constraint.
            ODataPathRouteConstraint routeConstraint = new ODataPathRouteConstraint(routeName);

            // Get constraint resolver.
            IInlineConstraintResolver inlineConstraintResolver = builder
                .ServiceProvider
                .GetRequiredService<IInlineConstraintResolver>();

            // Resolve HTTP handler, create the OData route and register it.
            routePrefix = RemoveTrailingSlash(routePrefix);
            ODataRoute route = new ODataRoute(builder.DefaultHandler, routeName, routePrefix, routeConstraint, inlineConstraintResolver);
            builder.Routes.Add(route);

            // Add a mapping between the route prefix and the EDM model.
            //options.ModelManager.AddModel(routePrefix, model);

            return route;
        }

        /// <summary>
        /// Maps the specified OData route and the OData route attributes.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapODataServiceRoute(this IRouteBuilder builder, string routeName,
            string routePrefix, IEdmModel model)
        {
            return builder.MapODataServiceRoute(routeName, routePrefix, model, containerBuilder =>
                containerBuilder.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => model)
                       .AddService<IEnumerable<IODataRoutingConvention>>(Microsoft.OData.ServiceLifetime.Singleton, sp =>
                           ODataRoutingConventions.CreateDefaultWithAttributeRouting(routeName, builder)));
        }

        /// <summary>
        /// Maps the specified OData route.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="routeName">The name of the route to map.</param>
        /// <param name="routePrefix">The prefix to add to the OData route's path template.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler"/> to use for parsing the OData path.</param>
        /// <param name="routingConventions">
        /// The OData routing conventions to use for controller and action selection.
        /// </param>
        /// <returns>The added <see cref="ODataRoute"/>.</returns>
        public static ODataRoute MapODataServiceRoute(this IRouteBuilder builder, string routeName,
            string routePrefix, IEdmModel model, IODataPathHandler pathHandler,
            IEnumerable<IODataRoutingConvention> routingConventions)
        {
            return builder.MapODataServiceRoute(routeName, routePrefix, model, containerBuilder =>
                containerBuilder.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => model)
                       .AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => pathHandler)
                       .AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => routingConventions.ToList().AsEnumerable()));
        }

        /// <summary>
        /// Remote the trailing slash from a route string.
        /// </summary>
        /// <param name="routePrefix"></param>
        /// <returns></returns>
        private static string RemoveTrailingSlash(string routePrefix)
        {
            if (!String.IsNullOrEmpty(routePrefix))
            {
                int prefixLastIndex = routePrefix.Length - 1;
                if (routePrefix[prefixLastIndex] == '/')
                {
                    // Remove the last trailing slash if it has one.
                    routePrefix = routePrefix.Substring(0, routePrefix.Length - 1);
                }
            }

            return routePrefix;
        }

        // TODO: Batch?
    }
}
