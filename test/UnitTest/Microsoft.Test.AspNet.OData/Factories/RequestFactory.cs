// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE1x
using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
#else
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData;
#endif

namespace Microsoft.Test.AspNet.OData.Factories
{
    /// <summary>
    /// A class to create HttpRequest[Message].
    /// </summary>
    public class RequestFactory
    {
        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
#if NETCORE1x
        public static HttpRequest Create(Action<IContainerBuilder> configAction = null)
        {
            // Add the options services.
            string routeName = "OData";
            IRouteBuilder config = RoutingConfigurationFactory.CreateWithRootContainer(routeName, configAction);

            // Create a new context and assign the services.
            HttpContext context = new DefaultHttpContext();
            context.RequestServices = config.ServiceProvider;

            // Ensure there is route data for the routing tests.
            var routeContext = new RouteContext(context);
            context.Features[typeof(IRoutingFeature)] = new RoutingFeature()
            {
                RouteData = routeContext.RouteData,
            };

            // Assign the route and get the request container, which will initialize
            // the request container if one does not exists.
            HttpRequest request = context.Request;
            request.ODataFeature().RouteName = routeName;
            request.GetRequestContainer();

            // Get request and return it.
            return request;
        }
#else
        public static HttpRequestMessage Create()
        {
            var request = new HttpRequestMessage();
            request.EnableODataDependencyInjectionSupport();
            return request;
        }
#endif

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
#if NETCORE1x
        public static HttpRequest Create(HttpMethod method, string uri, Action<IContainerBuilder> configAction = null)
        {
            HttpRequest request = Create(configAction);
            request.Method = method.ToString();

            Uri requestUri = new Uri(uri);
            request.Scheme = requestUri.Scheme;
            request.Host = requestUri.IsDefaultPort ?
                new HostString(requestUri.Host) :
                new HostString(requestUri.Host, requestUri.Port);
            request.QueryString = new QueryString(requestUri.Query);
            request.Path = new PathString(requestUri.AbsolutePath);

            return request;
        }
#else
        public static HttpRequestMessage Create(HttpMethod method, string uri)
        {
            var request = new HttpRequestMessage(method, uri);
            request.EnableODataDependencyInjectionSupport();
            return request;
        }
#endif

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
#if NETCORE1x
        public static HttpRequest Create(HttpMethod method, string uri, IRouteBuilder routeBuilder, string routeName = null)
        {
            HttpRequest request = RequestFactory.Create(method, uri);
            request.HttpContext.RequestServices = routeBuilder.ApplicationBuilder.ApplicationServices;

            IRouter defaultRoute = routeBuilder.Routes.FirstOrDefault();
            RouteData routeData = new RouteData();
            routeData.Routers.Add(defaultRoute);

            var mockAction = new Mock<ActionDescriptor>();
            ActionDescriptor actionDescriptor = mockAction.Object;

            ActionContext actionContext = new ActionContext(request.HttpContext, routeData, actionDescriptor);

            IActionContextAccessor actionContextAccessor = request.HttpContext.RequestServices.GetRequiredService<IActionContextAccessor>();
            actionContextAccessor.ActionContext = actionContext;

            if (!string.IsNullOrEmpty(routeName))
            {
                request.ODataFeature().RouteName = routeName;
            }

            return request;
        }
#else
        public static HttpRequestMessage Create(HttpMethod method, string uri, HttpConfiguration config, string routeName = null)
        {
            var request = new HttpRequestMessage(method, uri);
            request.SetConfiguration(config);

            if (!string.IsNullOrEmpty(routeName))
            {
                request.EnableODataDependencyInjectionSupport(routeName);
            }

            return request;
        }
#endif

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
#if NETCORE
        public static HttpRequest CreateFromModel(IEdmModel model, string uri = "http://localhost", string routeName = "Route", ODataPath path = null)
        {
            var configuration = RoutingConfigurationFactory.CreateWithRootContainer(routeName);
            configuration.MapODataServiceRoute(routeName, null, model);

            var request = RequestFactory.Create(HttpMethod.Get, uri, configuration, routeName);

            if (path != null)
            {
                request.ODataFeature().Path = path;
            }

            return request;
        }
#else
        public static HttpRequestMessage CreateFromModel(IEdmModel model, string uri = "http://localhost", string routeName = "Route", ODataPath path = null)
        {
            var configuration = RoutingConfigurationFactory.Create();
            configuration.MapODataServiceRoute(routeName, null, model);

            var request = RequestFactory.Create(HttpMethod.Get, uri, configuration, routeName);

            if (path != null)
            {

            }
            request.ODataProperties().Path = path;
            return request;
        }
#endif
    }
}
