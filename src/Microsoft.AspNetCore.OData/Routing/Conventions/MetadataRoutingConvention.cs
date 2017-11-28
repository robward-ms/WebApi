﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles OData metadata requests.
    /// </summary>
    public partial class MetadataRoutingConvention : IODataRoutingConvention
    {
        /// <inheritdoc/>
        public ControllerActionDescriptor SelectAction(RouteContext routeContext)
        {
            // Get a IActionDescriptorCollectionProvider from the global service provider.
            IActionDescriptorCollectionProvider actionCollectionProvider =
                routeContext.HttpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();
            Contract.Assert(actionCollectionProvider != null);

            ODataPath odataPath = routeContext.HttpContext.ODataFeature().Path;
            HttpRequest request = routeContext.HttpContext.Request;

            SelectControllerResult controllerResult = SelectControllerImpl(
                odataPath,
                new WebApiRequestMessage(request));

            if (controllerResult != null)
            {
                IEnumerable<ControllerActionDescriptor> actionDescriptors = actionCollectionProvider
                    .ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
                    .Where(c => c.ControllerName == controllerResult.ControllerName);

                string actionName = SelectActionImpl(
                    odataPath,
                    new WebApiControllerContext(routeContext, controllerResult),
                    new WebApiActionMap(actionDescriptors));

                if (!String.IsNullOrEmpty(actionName))
                {
                    return actionDescriptors.FirstOrDefault(
                        c => String.Equals(c.ActionName, actionName, StringComparison.OrdinalIgnoreCase));
                }
            }

            return null;
        }
    }
}