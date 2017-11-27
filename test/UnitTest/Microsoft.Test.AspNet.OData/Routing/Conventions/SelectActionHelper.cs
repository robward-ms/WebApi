// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Collections.Generic;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
#else
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Test.AspNet.OData.Factories;
using Microsoft.Test.AspNet.OData.TestCommon;
using Moq;
using Xunit;
#endif

namespace Microsoft.Test.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// Helper to create parameters for NavigationSourceRoutingConvention SelectAction().
    /// </summary>
    public static class SelectActionHelper
    {
#if NETCORE
        internal static IEnumerable<ControllerActionDescriptor> CreateActionMap(string key = null)
        {
            List<ControllerActionDescriptor> actionMap = new List<ControllerActionDescriptor>();
            ControllerActionDescriptor descriptor = new ControllerActionDescriptor();
            actionMap.Add(descriptor);

            if (!string.IsNullOrEmpty(key))
            {
                descriptor.ActionName = key;
            }

            return actionMap;
        }

        internal static string SelectAction(NavigationSourceRoutingConvention convention, ODataPath odataPath, HttpRequest request, IEnumerable<ControllerActionDescriptor> actionMap, string controllerName = "ControllerName")
        {
            // COnstruct parameters.
            RouteContext routeContext = new RouteContext(request.HttpContext);
            routeContext.HttpContext.ODataFeature().Path = odataPath;

            SelectControllerResult controllerResult = new SelectControllerResult(controllerName, null);

            // Select the action.
            string result = convention.SelectAction(routeContext, controllerResult, actionMap);

            // Copy route data to the context. In the real pipeline, this occurs in
            // RouterMiddleware.cs after the request has been routed.
            request.HttpContext.Features[typeof(IRoutingFeature)] = new RoutingFeature()
            {
                RouteData = routeContext.RouteData,
            };

            return result;
        }

        internal static RouteData GetRouteData(HttpRequest request)
        {
            // Get route data from the context. In the real pipeline, RouterMiddleware.cs 
            // copied this from the route context after the request has been routed.
            return request.HttpContext.GetRouteData();
        }
#else
        internal static ILookup<string, HttpActionDescriptor> CreateActionMap(string key = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                return new HttpActionDescriptor[0].ToLookup(desc => (string)null);
            }

            return new HttpActionDescriptor[1].ToLookup(desc => key);
        }

        internal static string SelectAction(NavigationSourceRoutingConvention convention, ODataPath odataPath, HttpRequestMessage request, ILookup<string, HttpActionDescriptor> actionMap, string controllerName = "ControllerName")
        {
            // Construct parameters.
            HttpRequestContext requestContext = new HttpRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                Request = request,
                RequestContext = requestContext,
                RouteData = new HttpRouteData(new HttpRoute())
            };
            controllerContext.Request.SetRequestContext(requestContext);

            // Select the action.
            string result = convention.SelectAction(odataPath, controllerContext, actionMap);
            return result;
        }

        internal static IHttpRouteData GetRouteData(HttpRequestMessage request)
        {
            return request.GetRouteData();
        }
#endif
    }
}
