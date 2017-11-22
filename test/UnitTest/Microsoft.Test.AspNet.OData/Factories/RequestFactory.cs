// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE1x
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData;
#else
using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
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
#if !NETCORE1x
        public static HttpRequestMessage Create()
        {
            var request = new HttpRequestMessage();
            request.EnableODataDependencyInjectionSupport();
            return request;
        }
#else
        public static HttpRequest Create(Action<IContainerBuilder> configAction = null)
        {
            IContainerBuilder builder = new DefaultContainerBuilder();
            builder.AddService<ODataOptions, ODataOptions>(Microsoft.OData.ServiceLifetime.Singleton);
            builder.AddService<DefaultQuerySettings, DefaultQuerySettings>(Microsoft.OData.ServiceLifetime.Singleton);
            if (configAction != null)
            {
                configAction(builder);
            }

            HttpContext context = new DefaultHttpContext();

            IServiceProvider provider = builder.BuildContainer();
            context.ODataFeature().RequestContainer = provider;
            context.RequestServices = provider;

            HttpRequest request = context.Request;
            return request;
        }
#endif

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
#if !NETCORE1x
        public static HttpRequestMessage Create(HttpMethod method, string uri)
        {
            var request = new HttpRequestMessage(method, uri);
            request.EnableODataDependencyInjectionSupport();
            return request;
        }
#else
        public static HttpRequest Create(HttpMethod method, string uri, Action<IContainerBuilder> configAction = null)
        {
            HttpRequest request = Create(configAction);
            request.Method = method.ToString();

            Uri requestUri = new Uri(uri);
            request.Host = new HostString(requestUri.Host, requestUri.Port);
            request.Scheme = requestUri.Scheme;

            return request;
        }
#endif

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
#if !NETCORE1x
        public static HttpRequestMessage Create(HttpMethod method, string uri, HttpConfiguration config, string routeName = null)
        {
            var request = new HttpRequestMessage(method, uri);
            request.SetConfiguration(config);

            if (!string.IsNullOrEmpty(routeName))
            {
                request.EnableODataDependencyInjectionSupport(routeName);
            }
            else
            {
                request.EnableODataDependencyInjectionSupport("OData");
            }

            return request;
        }
#else
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
                request.CreateRequestContainer(routeName);
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
#else
        public static HttpRequestMessage CreateFromModel(IEdmModel model, string uri = "http://localhost", string routeName = "Route", ODataPath path = null)
#endif
        {
            var configuration = RoutingConfigurationFactory.Create();
            configuration.MapODataServiceRoute(routeName, null, model);

            var request = RequestFactory.Create(HttpMethod.Get, uri, configuration, routeName);

            if (path != null)
            {

            }
#if NETCORE
            request.ODataFeature().Path = path;
#else
            request.ODataProperties().Path = path;
#endif

            return request;
        }
    }
}
