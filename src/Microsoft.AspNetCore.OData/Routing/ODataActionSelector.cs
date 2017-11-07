// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IActionSelector"/> that uses the server's OData routing conventions
    /// to select an action for OData requests.
    /// </summary>
    public class ODataActionSelector : IActionSelector
    {
        private readonly IActionSelector _innerSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataActionSelector" /> class.
        /// </summary>
        /// <param name="serviceProvider">IServiceProvider instance from dependency injection.</param>
        /// <param name="actionDescriptorCollectionProvider">IActionDescriptorCollectionProvider instance from dependency injection.</param>
        /// <param name="actionConstraintProviders">ActionConstraintCache instance from dependency injection.</param>
        /// <param name="loggerFactory">ILoggerFactory instance from dependency injection.</param>
        public ODataActionSelector(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            ActionConstraintCache actionConstraintProviders,
            ILoggerFactory loggerFactory)
        {
            _innerSelector = new ActionSelector(actionDescriptorCollectionProvider, actionConstraintProviders, loggerFactory);
        }

        /// <inheritdoc />
        public IReadOnlyList<ActionDescriptor> SelectCandidates(RouteContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            HttpRequest request = context.HttpContext.Request;
            IEnumerable<IODataRoutingConvention> routingConventions = request.GetRoutingConventions();
            ODataPath odataPath = context.HttpContext.ODataFeature().Path;
            RouteData routeData = context.RouteData;

            if (odataPath == null || routingConventions == null || routeData.Values.ContainsKey(ODataRouteConstants.Action))
            {
                // If there is no path, no routing conventions or there is already and indication we routed it,
                // let the _innerSelector handle the request.
                return _innerSelector.SelectCandidates(context);
            }

            foreach (IODataRoutingConvention convention in routingConventions)
            {
                ControllerActionDescriptor actionDescriptor = convention.SelectAction(context);
                if (actionDescriptor != null)
                {
                    context.RouteData.Values[ODataRouteConstants.Action] = actionDescriptor.ActionName;
                    List<ActionDescriptor> list = new List<ActionDescriptor> { actionDescriptor };
                    return list.AsReadOnly();
                }
            }

            return _innerSelector.SelectCandidates(context);
        }

        /// <inheritdoc />
        public ActionDescriptor SelectBestCandidate(RouteContext context, IReadOnlyList<ActionDescriptor> candidates)
        {
            RouteData routeData = context.RouteData;
            ODataPath odataPath = context.HttpContext.ODataFeature().Path;
            if (odataPath != null && routeData.Values.ContainsKey(ODataRouteConstants.Action))
            {
                // If there is a path and is already and indication we routed it,
                // select the first candidate; we should have only put one candidate in the list.
                return candidates.First();
            }

            return _innerSelector.SelectBestCandidate(context, candidates);
        }
    }
}
