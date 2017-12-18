// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Collections.Generic;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#else
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

#endif

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public abstract class TestEntitySetRoutingConvention : EntitySetRoutingConvention
    {
        internal abstract string SelectAction(ODataPath odataPath, WebApiControllerContext controllerContext, WebApiActionMap actionMap);

#if NETCORE
        internal override string SelectAction(RouteContext routeContext, SelectControllerResult controllerResult, IEnumerable<ControllerActionDescriptor> actionDescriptors)
        {
            ODataPath path = routeContext.HttpContext.Request.ODataFeature().Path;
            WebApiControllerContext internalControllerContext = new WebApiControllerContext(routeContext, controllerResult);
            WebApiActionMap internalActionMap = new WebApiActionMap(actionDescriptors);
            return SelectAction(path, internalControllerContext, internalActionMap);
        }
#else
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            WebApiControllerContext internalControllerContext = new WebApiControllerContext(controllerContext);
            WebApiActionMap internalActionMap = new WebApiActionMap(actionMap);
            return SelectAction(odataPath, internalControllerContext, internalActionMap);
        }
#endif
    }
}