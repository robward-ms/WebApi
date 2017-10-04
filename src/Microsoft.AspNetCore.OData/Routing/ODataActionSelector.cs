// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.OData.Adapters;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IActionSelector"/> that uses the server's OData routing conventions
    /// to select an action for OData requests.
    /// </summary>
    public class ODataActionSelector : IActionSelector
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IActionSelector _selector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataActionSelector" /> class.
        /// </summary>
        /// <param name="serviceProvider">IServiceProvider instance from dependency injection.</param>
        /// <param name="decisionTreeProvider">IActionSelectorDecisionTreeProvider instance from dependency injection.</param>
        /// <param name="actionConstraintProviders">ActionConstraintCache instance from dependency injection.</param>
        /// <param name="loggerFactory">ILoggerFactory instance from dependency injection.</param>
        public ODataActionSelector(IServiceProvider serviceProvider,
            IActionSelectorDecisionTreeProvider decisionTreeProvider,
            ActionConstraintCache actionConstraintProviders,
            ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider;
            _selector = new ActionSelector(decisionTreeProvider, actionConstraintProviders, loggerFactory);
        }

        /// <inheritdoc />
        public IReadOnlyList<ActionDescriptor> SelectCandidates(RouteContext routeContext)
        {
            if (routeContext.HttpContext.ODataFeature().IsValidODataRequest)
            {
                ODataOptions options = _serviceProvider.GetRequiredService<IOptions<ODataOptions>>().Value;

                IActionDescriptorCollectionProvider actionCollectionProvider =
                    routeContext.HttpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();
                Contract.Assert(actionCollectionProvider != null);

                ActionDescriptor actionDescriptor = null;
                foreach (var convention in options.RoutingConventions)
                {
                    ODataPath odataPath = routeContext.HttpContext.ODataFeature().Path;
                    HttpRequest request = routeContext.HttpContext.Request;

                    actionDescriptor = convention.SelectAction(routeContext);
                    if (actionDescriptor != null)
                    {
                        List<ActionDescriptor> list = new List<ActionDescriptor> { actionDescriptor };
                        return list.AsReadOnly();
                    }
                }
            }

            return _selector.SelectCandidates(routeContext);
        }

        /// <inheritdoc />
        public ActionDescriptor SelectBestCandidate(RouteContext context, IReadOnlyList<ActionDescriptor> candidates)
        {
            if (context.HttpContext.ODataFeature().IsValidODataRequest)
            {
                // We shoudl have only put one candidate in the list, select it.
                return candidates.First();
            }

            return _selector.SelectBestCandidate(context, candidates);
        }
    }
}
