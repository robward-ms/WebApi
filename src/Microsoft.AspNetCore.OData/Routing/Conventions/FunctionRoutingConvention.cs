// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData.Adapters;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles function invocations.
    /// </summary>
    public partial class FunctionRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <inheritdoc/>
        internal override string SelectAction(RouteContext routeContext, SelectControllerResult controllerResult, IEnumerable<ControllerActionDescriptor> actionDescriptors)
        {
            if (routeContext == null)
            {
                throw Error.ArgumentNull("routeContext");
            }

            if (controllerResult == null)
            {
                throw Error.ArgumentNull("controllerResult");
            }

            if (actionDescriptors == null)
            {
                throw Error.ArgumentNull("actionDescriptors");
            }

            return SelectActionImpl(
                routeContext.HttpContext.ODataFeature().Path,
                new WebApiControllerContext(routeContext, controllerResult),
                new WebApiActionMap(actionDescriptors));
        }
    }
}
